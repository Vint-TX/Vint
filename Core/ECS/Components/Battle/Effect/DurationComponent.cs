using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect;

[ProtocolId(5192591761194414739)]
public class DurationComponent(
    DateTimeOffset startedTime
) : IComponent {
    public DateTimeOffset StartedTime { get; private set; } = startedTime;
}