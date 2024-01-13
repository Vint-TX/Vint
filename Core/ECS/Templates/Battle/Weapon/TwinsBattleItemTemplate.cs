using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Weapon.Types;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(583528765588657091)]
public class TwinsBattleItemTemplate : BulletWeaponTemplate {
    public IEntity Create(IEntity tank, BattlePlayer battlePlayer) {
        IEntity entity = base.Create("garage/weapon/twins", tank, battlePlayer);

        entity.AddComponent(new TwinsComponent());
        return entity;
    }
}