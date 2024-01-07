using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Tank;

[ProtocolId(636047163591561471)]
public class HullSkinBattleItemTemplate : EntityTemplate {
    public IEntity Create(IEntity skin, IEntity tank) => Entity(skin.TemplateAccessor!.ConfigPath,
        builder =>
            builder
                .AddComponent(new HullSkinBattleItemComponent())
                .AddComponent(tank.GetComponent<UserGroupComponent>())
                .AddComponent(tank.GetComponent<BattleGroupComponent>())
                .AddComponent(tank.GetComponent<TankGroupComponent>())
                .AddComponent(skin.GetComponent<MarketItemGroupComponent>()));
}