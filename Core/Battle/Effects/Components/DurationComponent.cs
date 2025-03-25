using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components;

[ProtocolId(5192591761194414739)]
public class DurationComponent(
    DateTimeOffset startedTime
) : IComponent {
    public DateTimeOffset StartedTime { get; private set; } = startedTime;
}
