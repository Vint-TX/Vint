using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(-5749845294664286691), ClientAddable, ClientRemovable]
public class ShaftIdleStateComponent : IComponent {
    public Task Added(IPlayerConnection connection, IEntity entity) {
        if (connection.LobbyPlayer?.Tanker?.Tank.WeaponHandler is not ShaftWeaponHandler shaft)
            return Task.CompletedTask;

        shaft.Reset();
        return Task.CompletedTask;
    }
}
