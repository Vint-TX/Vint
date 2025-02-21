using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(6803807621463709653), ClientAddable, ClientRemovable]
public class WeaponStreamShootingComponent : IComponent {
    public DateTimeOffset? StartShootingTime { get; private set; }
    public int Time { get; private set; }

    public Task Added(IPlayerConnection connection, IEntity entity) {
        if (connection.LobbyPlayer?.Tanker?.Tank.WeaponHandler is not VulcanWeaponHandler vulcan ||
            !entity.HasComponent<VulcanComponent>())
            return Task.CompletedTask;

        vulcan.ShootingStartTime ??= DateTimeOffset.UtcNow;
        return Task.CompletedTask;
    }

    public Task Removed(IPlayerConnection connection, IEntity entity) {
        if (connection.LobbyPlayer?.Tanker?.Tank.WeaponHandler is not VulcanWeaponHandler vulcan ||
            !entity.HasComponent<VulcanComponent>())
            return Task.CompletedTask;

        vulcan.ShootingStartTime = null;
        vulcan.LastOverheatingUpdate = null;
        vulcan.IncarnationIdToHitTime.Clear();
        vulcan.IncarnationIdToLastHitTime.Clear();
        return Task.CompletedTask;
    }
}
