using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Round;

[ProtocolId(6921761768819133913)]
public class RoundUserStatisticsComponent(
    int place,
    int scoreWithoutBonuses,
    int kills,
    int killAssists,
    int deaths
) : IComponent {
    public int Place { get; set; } = place;
    public int ScoreWithoutBonuses { get; set; } = scoreWithoutBonuses;
    public int Kills { get; set; } = kills;
    public int KillAssists { get; set; } = killAssists;
    public int Deaths { get; set; } = deaths;
}