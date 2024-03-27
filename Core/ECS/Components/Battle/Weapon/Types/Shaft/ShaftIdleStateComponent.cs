using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(-5749845294664286691)]
public class ShaftIdleStateComponent : IComponent {
    public void Added(IPlayerConnection connection, IEntity entity) {
        if (connection.BattlePlayer?.Tank?.WeaponHandler is not ShaftWeaponHandler shaft) return;

        shaft.Reset();
    }
}