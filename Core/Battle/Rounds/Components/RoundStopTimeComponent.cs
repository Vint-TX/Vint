using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Rounds.Components;

[ProtocolId(92197374614905239)]
public class RoundStopTimeComponent(
    DateTimeOffset stopTime
) : IComponent {
    public DateTimeOffset StopTime { get; set; } = stopTime;
}
