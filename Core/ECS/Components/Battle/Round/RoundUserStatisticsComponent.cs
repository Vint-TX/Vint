using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Round;

[ProtocolId(6921761768819133913)]
public class RoundUserStatisticsComponent : IComponent {
    public int Place { get; set; }
    public int ScoreWithoutBonuses { get; set; }
    public int Kills { get; set; }
    public int KillAssists { get; set; }
    public int Deaths { get; set; }
}
