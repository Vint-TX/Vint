using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Incarnation;

[ProtocolId(1491549293967)]
public class TankIncarnationKillStatisticsComponent(
    int kills
) : IComponent {
    public int Kills { get; set; } = kills;
}