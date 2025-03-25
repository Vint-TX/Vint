using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Battle.Weapon.Types;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(525358843506658817)]
public class FreezeBattleItemTemplate : StreamWeaponTemplate {
    public IEntity Create(IEntity tank, Tanker tanker) {
        IEntity entity = Create("garage/weapon/freeze", tank, tanker);

        entity.AddComponent<FreezeComponent>();
        return entity;
    }
}
