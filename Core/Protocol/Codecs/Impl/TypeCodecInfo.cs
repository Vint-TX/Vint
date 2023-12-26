using Vint.Core.Utils;

namespace Vint.Core.Protocol.Codecs.Impl;

public class TypeCodecInfo(
    Type type,
    bool nullable = false,
    bool varied = false,
    HashSet<Attribute>? attributes = null
) : ITypeCodecInfo, IEquatable<TypeCodecInfo> {
    public bool Equals(TypeCodecInfo? other) => this == other;

    public Type Type { get; } = type;
    public bool Nullable { get; } = nullable;
    public bool Varied { get; } = varied;
    public HashSet<Attribute> Attributes { get; } = attributes ?? [];

    public override bool Equals(object? obj) => Equals(obj as TypeCodecInfo);

    public static bool operator ==(TypeCodecInfo? x, TypeCodecInfo? y) {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;

        return x.Type == y.Type &&
               x.Nullable == y.Nullable &&
               x.Varied == y.Varied &&
               x.Attributes.SetEquals(y.Attributes);
    }

    public static bool operator !=(TypeCodecInfo? x, TypeCodecInfo? y) => !(x == y);

    public override int GetHashCode() {
        unchecked {
            int hash = 17;

            hash = hash * 23 + Type.GetHashCode();
            hash = hash * 23 + Nullable.GetHashCode();
            hash = hash * 23 + Varied.GetHashCode();
            hash = Attributes.Aggregate(hash, (current, attribute) => current * 23 + attribute.GetHashCode());

            return hash;
        }
    }

    public override string ToString() {
        string typeName = Type.Name;

        if (Type.IsGenericType) {
            typeName = typeName[..^2];
            Type[] genericTypeArguments = Type.GenericTypeArguments;

            typeName = $"{typeName}<{genericTypeArguments.First().Name}";

            if (genericTypeArguments.Length > 1) {
                typeName = genericTypeArguments
                    .Skip(1)
                    .Aggregate(typeName,
                        (current, genericArg) =>
                            $"{current}, {genericArg.Name}");
            }

            typeName = $"{typeName}>";
        }

        return $"TypeCodecInfo {{ " +
               $"Type: {typeName}; " +
               $"Nullable: {Nullable}; " +
               $"Varied: {Varied}; " +
               $"Attributes {{ {Attributes.ToString(false)} }} }}";
    }
}