using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(636287143924344191)]
public class WeaponPaintBattleItemTemplate : EntityTemplate {
    public IEntity Create(IEntity cover, IEntity tank) => Entity(cover.TemplateAccessor!.ConfigPath, builder =>
            builder
                .AddComponent(new WeaponPaintBattleItemComponent())
                .AddComponent(tank.GetComponent<UserGroupComponent>())
                .AddComponent(tank.GetComponent<BattleGroupComponent>())
                .AddComponent(tank.GetComponent<TankGroupComponent>())
                .AddComponent(cover.GetComponent<MarketItemGroupComponent>()));
}