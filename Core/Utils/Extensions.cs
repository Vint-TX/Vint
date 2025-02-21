using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Enums;
using Vint.Core.Exceptions;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Utils;

public static class Extensions {
    static ConcurrentDictionary<PropertyInfo, bool> NullabilityPool { get; } = new();

    public static long GetProtocolId(this Type type) =>
        type.GetCustomAttribute<ProtocolIdAttribute>()
            ?.Id ??
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

    public static TList Shuffle<TList>(this TList list) where TList : IList {
        for (int i = list.Count - 1; i > 0; i--) {
            int n = Random.Shared.Next(i + 1);
            (list[i], list[n]) = (list[n], list[i]);
        }

        return list;
    }

    public static TElement RandomElement<TElement>(this IList<TElement> list) =>
        list[Random.Shared.Next(list.Count)];

    public static bool IsNullable(this PropertyInfo property) {
        if (NullabilityPool.TryGetValue(property, out bool isNullable))
            return isNullable;

        NullabilityInfo nullabilityInfo = new NullabilityInfoContext().Create(property);
        isNullable = nullabilityInfo.ReadState == NullabilityState.Nullable || nullabilityInfo.WriteState == NullabilityState.Nullable;

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

        return list.Count == 0
            ? "Empty"
            : string.Join(", ", list.Select(obj => extended ? $"{obj}" : obj!.GetType().Name));
    }

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

    [Pure]
    public static bool HasDuplicates<TSource>(this IEnumerable<TSource> source) {
        using IEnumerator<TSource> enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
            return false;

        HashSet<TSource> set = [];

        do {
            if (!set.Add(enumerator.Current))
                return true;
        } while (enumerator.MoveNext());

        return false;
    }

    [Pure]
    [LinqTunnel]
    public static bool HasDuplicatesBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector) =>
        HasDuplicates(source.Select(selector));

    public static bool IsTeamMode(this BattleMode mode) => mode is
        BattleMode.TDM or
        BattleMode.CTF or
        BattleMode.CP;
}
