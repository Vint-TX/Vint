using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(971549724137995758), ClientAddable, ClientRemovable]
public class StreamWeaponWorkingComponent : IComponent {
    public int Time { get; private set; }

    public async Task Added(IPlayerConnection connection, IEntity entity) {
        BattleTank? tank = connection.LobbyPlayer?.Tanker?.Tank;

        if (tank == null)
            return;

        foreach (IShotModule shotModule in tank.Modules.OfType<IShotModule>())
            await shotModule.OnShot();
    }

    public Task Removed(IPlayerConnection connection, IEntity entity) {
        if (connection.LobbyPlayer?.Tanker?.Tank.WeaponHandler is not StreamWeaponHandler streamHandler)
            return Task.CompletedTask;

        streamHandler.IncarnationIdToHitTime.Clear();
        streamHandler.IncarnationIdToLastHitTime.Clear();
        return Task.CompletedTask;
    }
}
