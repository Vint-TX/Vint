namespace Vint.Core.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ProtocolDictionaryAttribute(
    ProtocolCollectionAttribute key,
    ProtocolCollectionAttribute value
) : Attribute {
    public ProtocolDictionaryAttribute() : this(new ProtocolCollectionAttribute(), new ProtocolCollectionAttribute()) { }

    public ProtocolCollectionAttribute Key => key;
    public ProtocolCollectionAttribute Value => value;

    public static ProtocolDictionaryAttribute Default => new();

    public override string ToString() => $"ProtocolDictionary {{ Key: {Key}; Value: {Value} }}";
}