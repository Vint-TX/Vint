using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect;

[ProtocolId(482294559116673084)]
public class DurationConfigComponent(
    TimeSpan duration
) : IComponent {
    public long Duration { get; set; } = Convert.ToInt64(duration.TotalMilliseconds);
}