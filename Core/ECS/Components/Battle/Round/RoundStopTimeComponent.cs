using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Round;

[ProtocolId(92197374614905239)]
public class RoundStopTimeComponent(
    DateTimeOffset stopTime
) : IComponent {
    public DateTimeOffset StopTime { get; set; } = stopTime;
}