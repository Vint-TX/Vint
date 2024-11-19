namespace Vint.Core.Protocol.Attributes;

public class ProtocolTimeKindAttribute<T>(TimeSpanKind kind) : ProtocolTimeKindAttribute(typeof(T), kind);

[AttributeUsage(AttributeTargets.Property)]
public class ProtocolTimeKindAttribute(Type numberType, TimeSpanKind kind) : Attribute {
    public Type NumberType { get; } = numberType;
    public TimeSpanKind Kind { get; } = kind;
}

public enum TimeSpanKind {
    Ticks,
    Microseconds,
    Milliseconds,
    Seconds,
    Minutes,
    Hours,
    Days
}
