namespace Vint.Core.Protocol.Codecs.Info;

public readonly struct CodecInfo(
    Type type,
    bool nullable = false,
    bool varied = false
) : ICodecInfo, IEquatable<CodecInfo> {
    public Type Type { get; } = type;

    public bool Nullable { get; } = nullable;

    public bool Varied { get; } = varied;

    public override bool Equals(object? obj) => Equals(obj is CodecInfo info ? info : default);

    public bool Equals(CodecInfo other) => this == other;

    public override int GetHashCode() => HashCode.Combine(Type, Nullable, Varied);

    public override string ToString() {
        string typeName = Type.Name;

        if (Type.IsGenericType) {
            typeName = typeName[..^2];
            Type[] genericTypeArguments = Type.GenericTypeArguments;

            typeName += $"<{genericTypeArguments.First().Name}";

            if (genericTypeArguments.Length > 1) {
                typeName = genericTypeArguments
                    .Skip(1)
                    .Aggregate(typeName,
                        (current, genericArg) =>
                            $"{current}, {genericArg.Name}");
            }

            typeName += '>';
        }

        return $"CodecInfo {{ " +
               $"Type: {typeName}; " +
               $"Nullable: {Nullable}; " +
               $"Varied: {Varied} }}";
    }

    public static bool operator ==(CodecInfo? x, CodecInfo? y) {
        if (!x.HasValue && !y.HasValue)
            return true;

        if (x.HasValue && !y.HasValue ||
            !x.HasValue && y.HasValue)
            return false;

        return x!.Value == y!.Value;
    }

    public static bool operator !=(CodecInfo? x, CodecInfo? y) => !(x == y);

    public static bool operator ==(CodecInfo x, CodecInfo y) =>
        x.Type == y.Type &&
        x.Nullable == y.Nullable &&
        x.Varied == y.Varied;

    public static bool operator !=(CodecInfo x, CodecInfo y) => !(x == y);
}
