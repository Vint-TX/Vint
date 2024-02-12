using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Team;

[ProtocolId(-2440064891528955383)]
public class TeamScoreComponent(
    int score = 0
) : IComponent {
    public int Score { get; set; } = score;
}