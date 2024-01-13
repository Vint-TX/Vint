using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Weapon.Splash;
using Vint.Core.ECS.Components.Battle.Weapon.Types;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(-8770103861152493981)]
public class ThunderBattleItemTemplate : DiscreteWeaponTemplate {
    public IEntity Create(IEntity tank, BattlePlayer battlePlayer) {
        IEntity entity = Create("garage/weapon/thunder", tank, battlePlayer);

        entity.AddComponent(new SplashImpactComponent(4f));
        entity.AddComponent(new SplashWeaponComponent(40f, 0f, 15f));
        entity.AddComponent(new ThunderComponent());

        return entity;
    }
}