using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(971549724137995758), ClientAddable, ClientRemovable]
public class StreamWeaponWorkingComponent : IComponent {
    public int Time { get; private set; }

    public Task Added(IPlayerConnection connection, IEntity entity) {
        if (!connection.InLobby || !connection.BattlePlayer!.InBattleAsTank) return Task.CompletedTask;

        foreach (IShotModule shotModule in connection.BattlePlayer.Tank!.Modules.OfType<IShotModule>())
            shotModule.OnShot();

        return Task.CompletedTask;
    }

    public Task Removed(IPlayerConnection connection, IEntity entity) {
        (connection.BattlePlayer?.Tank?.WeaponHandler as StreamWeaponHandler)?.IncarnationIdToHitTime.Clear();
        return Task.CompletedTask;
    }
}
