using Vint.Core.Database;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.User;

[ProtocolId(1522236020112)]
public class FavoriteEquipmentStatisticsComponent : IComponent {
    public FavoriteEquipmentStatisticsComponent(long playerId) {
        using DbConnection db = new();

        HullStatistics = db.Hulls
            .Where(hull => hull.PlayerId == playerId)
            .Select(hull => new { hull.Id, hull.BattlesPlayed })
            .ToDictionary(hull => hull.Id, hull => hull.BattlesPlayed);

        TurretStatistics = db.Weapons
            .Where(weapon => weapon.PlayerId == playerId)
            .Select(weapon => new { weapon.Id, weapon.BattlesPlayed })
            .ToDictionary(weapon => weapon.Id, weapon => weapon.BattlesPlayed);
    }

    public Dictionary<long, long> HullStatistics { get; private set; }

    public Dictionary<long, long> TurretStatistics { get; private set; }
}