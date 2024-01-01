namespace Vint.Core.Protocol.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ProtocolPositionAttribute(
    int position
) : Attribute {
    public int Position => position;

    public override string ToString() => $"ProtocolPosition {{ Position: {Position} }}";
}