using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Incarnation.Components;

[ProtocolId(1491549293967)]
public class TankIncarnationKillStatisticsComponent(
    int kills
) : IComponent {
    public int Kills { get; set; } = kills;
}
