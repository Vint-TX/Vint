using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect;

[ProtocolId(482294559116673084)]
public class DurationConfigComponent(
    TimeSpan duration
) : IComponent {
    [ProtocolTimeKind<long>(TimeSpanKind.Milliseconds)]
    public TimeSpan Duration { get; set; } = duration;
}
