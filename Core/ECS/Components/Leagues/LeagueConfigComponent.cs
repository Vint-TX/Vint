using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Leagues;

[ProtocolId(1502713060357)]
public class LeagueConfigComponent(
    int leagueIndex,
    double reputationToEnter
) : IComponent {
    public int LeagueIndex { get; private set; } = leagueIndex;
    public double ReputationToEnter { get; private set; } = reputationToEnter;
}