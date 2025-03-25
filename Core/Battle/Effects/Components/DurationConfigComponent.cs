using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components;

[ProtocolId(482294559116673084)]
public class DurationConfigComponent(
    TimeSpan duration
) : IComponent {
    public long Duration { get; set; } = (long)Math.Ceiling(duration.TotalMilliseconds);
}
