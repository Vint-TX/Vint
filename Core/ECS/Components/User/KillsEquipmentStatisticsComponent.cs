using Vint.Core.Database;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1499175516647)]
public class KillsEquipmentStatisticsComponent : IComponent {
    public KillsEquipmentStatisticsComponent(long playerId) {
        using DbConnection db = new();

        HullStatistics = db.Hulls
            .Where(hull => hull.PlayerId == playerId)
            .Select(hull => new { hull.Id, hull.Kills })
            .ToDictionary(hull => hull.Id, hull => hull.Kills);

        TurretStatistics = db.Weapons
            .Where(weapon => weapon.PlayerId == playerId)
            .Select(weapon => new { weapon.Id, weapon.Kills })
            .ToDictionary(weapon => weapon.Id, weapon => weapon.Kills);
    }

    public Dictionary<long, long> HullStatistics { get; private set; }
    public Dictionary<long, long> TurretStatistics { get; private set; }
}