using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Round;

[ProtocolId(3051892485776042754)]
public class RoundDisbalancedComponent(
    TeamColor loser,
    DateTimeOffset finishTime
) : IComponent {
    public DateTimeOffset FinishTime { get; private set; } = finishTime;
    public TeamColor Loser { get; private set; } = loser;
}
