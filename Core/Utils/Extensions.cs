using System.Buffers;
using System.Collections.Concurrent;
using System.Reflection;
using Vint.Core.ECS.Components;
using Vint.Core.Exceptions;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.Utils;

public static class Extensions {
    static ConcurrentDictionary<PropertyInfo, bool> NullabilityPool { get; } = new();

    public static ProtocolIdAttribute GetProtocolId(this Type type) =>
        type.GetCustomAttribute<ProtocolIdAttribute>() ??
        throw new ProtocolIdNotFoundException(type);

    public static List<Type> DumpInterfaces(this Type type) {
        if (!type.IsClass)
            throw new NotSupportedException("Type must be a class");

        HashSet<Type> allInterfaces = [..type.GetInterfaces()];
        Type? baseType = type.BaseType;

        if (baseType != null)
            allInterfaces.ExceptWith(baseType.GetInterfaces());

        return allInterfaces.ToList();
    }

    public static void CopyTo(this Stream input, Stream output, int limit) {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(256);
        int read;

        while (limit > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, limit))) > 0) {
            output.Write(buffer, 0, read);
            limit -= read;
        }

        ArrayPool<byte>.Shared.Return(buffer);
    }

    public static List<T> Shuffle<T>(this List<T> list) {
        int n = list.Count;

        while (n > 1) {
            n--;
            int k = Random.Shared.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }

        return list;
    }

    public static bool IsNullable(this PropertyInfo property) {
        if (NullabilityPool.TryGetValue(property, out bool isNullable))
            return isNullable;

        NullabilityInfo nullabilityInfo = new NullabilityInfoContext().Create(property);
        isNullable = nullabilityInfo.ReadState == NullabilityState.Nullable ||
                     nullabilityInfo.WriteState == NullabilityState.Nullable;

        NullabilityPool.TryAdd(property, isNullable);
        return isNullable;
    }

    public static bool IsList(this Type type) => type.IsGenericType &&
                                                 type.GetGenericTypeDefinition()
                                                     .IsAssignableFrom(typeof(List<>));

    public static bool IsDictionary(this Type type) => type.IsGenericType &&
                                                       type.GetGenericTypeDefinition()
                                                           .IsAssignableFrom(typeof(Dictionary<,>));

    public static bool IsHashSet(this Type type) => type.IsGenericType &&
                                                    type.GetGenericTypeDefinition()
                                                        .IsAssignableFrom(typeof(HashSet<>));

    public static string ToString<T>(this IEnumerable<T> enumerable, bool extended) {
        List<T> list = enumerable.ToList();

        return list.Count == 0 ? "Empty" : string.Join(", ", list.Select(obj => extended ? $"{obj}" : obj!.GetType().Name));
    }

    public static Task Catch(this Task task) => task.ContinueWith(t =>
            t.Exception!.Flatten().InnerExceptions.ToList().ForEach(Console.Error.WriteLine),
        TaskContinuationOptions.OnlyOnFaulted);

    public static T? SingleOrDefaultSafe<T>(this IEnumerable<T> enumerable, T? defaultValue = default) {
        try {
            return enumerable.SingleOrDefault(defaultValue);
        } catch {
            return defaultValue;
        }
    }

    public static T Clone<T>(this T self) where T : IComponent => (T)self.Clone();

    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable) {
        List<T> list = [];

        await foreach (T element in enumerable)
            list.Add(element);

        return list;
    }

    public static void RunTaskInBackground(Func<Task> task, Func<Exception, Task> @catch, bool longRunning = false) {
        Task.Factory.StartNew(async () => {
            try {
                await task();
            } catch (Exception e) {
                await @catch(e);
                throw;
            }
        }, longRunning ? TaskCreationOptions.LongRunning : TaskCreationOptions.None);
    }

    public static void RunTaskInBackground(Func<Task> task, Action<Exception> @catch, bool longRunning = false) {
        Task.Factory.StartNew(async () => {
            try {
                await task();
            } catch (Exception e) {
                @catch(e);
                throw;
            }
        }, longRunning ? TaskCreationOptions.LongRunning : TaskCreationOptions.None);
    }
}
