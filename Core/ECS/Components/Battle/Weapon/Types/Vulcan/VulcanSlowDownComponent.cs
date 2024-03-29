using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;

[ProtocolId(-6843896944033144903), ClientAddable, ClientRemovable]
public class VulcanSlowDownComponent : IComponent {
    public bool IsAfterShooting { get; private set; }
    public int ClientTime { get; private set; }

    public void Added(IPlayerConnection connection, IEntity entity) {
        if (!IsAfterShooting || connection.BattlePlayer?.Tank?.WeaponHandler is not VulcanWeaponHandler vulcan) return;

        vulcan.ShootingStartTime = null;
        vulcan.LastOverheatingUpdate = null;
    }
}