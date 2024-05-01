using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(6803807621463709653), ClientAddable, ClientRemovable]
public class WeaponStreamShootingComponent : IComponent {
    public DateTimeOffset? StartShootingTime { get; private set; }
    public int Time { get; private set; }

    public Task Added(IPlayerConnection connection, IEntity entity) {
        if (connection.BattlePlayer?.Tank?.WeaponHandler is not VulcanWeaponHandler vulcan) return Task.CompletedTask;

        vulcan.ShootingStartTime ??= DateTimeOffset.UtcNow;
        return Task.CompletedTask;
    }

    public Task Removed(IPlayerConnection connection, IEntity entity) {
        if (connection.BattlePlayer?.Tank?.WeaponHandler is not VulcanWeaponHandler vulcan) return Task.CompletedTask;

        vulcan.ShootingStartTime = null;
        vulcan.LastOverheatingUpdate = null;
        return Task.CompletedTask;
    }
}
