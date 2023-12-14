using System.Reflection;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.Utils;

public static class Extensions {
    public static ProtocolIdAttribute GetProtocolId(this Type type) => type.GetCustomAttribute<ProtocolIdAttribute>()!;

    public static List<Type> DumpInterfaces(this Type type) {
        if (!type.IsClass)
            throw new NotSupportedException("Type must be a class");

        HashSet<Type> allInterfaces = new(type.GetInterfaces());
        Type? baseType = type.BaseType;

        if (baseType != null)
            allInterfaces.ExceptWith(baseType.GetInterfaces());

        return allInterfaces.ToList();
    }

    public static void CopyTo(this Stream input, Stream output, int limit) {
        byte[] buffer = new byte[32768];
        int read;

        while (limit > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, limit))) > 0) {
            output.Write(buffer, 0, read);
            limit -= read;
        }
    }

    public static bool IsNullable(this PropertyInfo property) {
        NullabilityInfo nullabilityInfo = new NullabilityInfoContext().Create(property);

        return nullabilityInfo.ReadState == NullabilityState.Nullable ||
               nullabilityInfo.WriteState == NullabilityState.Nullable;
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
}