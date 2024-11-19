using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Round;

[ProtocolId(3051892485776042754)]
public class RoundDisbalancedComponent(
    TeamColor loser,
    TimeSpan initialDominationTimer,
    DateTimeOffset finishTime
) : IComponent {
    [ProtocolTimeKind<int>(TimeSpanKind.Seconds)]
    public TimeSpan InitialDominationTimerSec { get; private set; } = initialDominationTimer;
    public DateTimeOffset FinishTime { get; private set; } = finishTime;
    public TeamColor Loser { get; private set; } = loser;
}
