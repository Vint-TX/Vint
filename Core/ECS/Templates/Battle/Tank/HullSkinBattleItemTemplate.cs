using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Tank;

[ProtocolId(636047163591561471)]
public class HullSkinBattleItemTemplate : EntityTemplate {
    public IEntity Create(IEntity skin, IEntity tank) => Entity(skin.TemplateAccessor!.ConfigPath,
        builder => builder
            .AddComponent<HullSkinBattleItemComponent>()
            .AddComponentFrom<UserGroupComponent>(tank)
            .AddComponentFrom<BattleGroupComponent>(tank)
            .AddComponentFrom<TankGroupComponent>(tank)
            .AddComponentFrom<MarketItemGroupComponent>(skin));
}