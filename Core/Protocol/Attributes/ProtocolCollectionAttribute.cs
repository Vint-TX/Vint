namespace Vint.Core.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ProtocolCollectionAttribute(
    bool nullable = false,
    bool varied = false
) : Attribute {
    public bool Nullable => nullable;
    public bool Varied => varied;

    public static ProtocolCollectionAttribute Default => new();

    public override string ToString() => $"ProtocolCollection {{ Nullable: {Nullable}; Varied: {Varied} }}";
}