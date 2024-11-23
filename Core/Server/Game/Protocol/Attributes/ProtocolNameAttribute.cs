namespace Vint.Core.Server.Game.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ProtocolNameAttribute(
    string name
) : Attribute {
    public string Name => name;

    public override string ToString() => $"ProtocolName {{ Name: {Name} }}";
}
