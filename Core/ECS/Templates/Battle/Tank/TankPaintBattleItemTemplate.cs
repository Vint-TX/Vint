using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Tank;

[ProtocolId(1437375358285)]
public class TankPaintBattleItemTemplate : EntityTemplate {
    public IEntity Create(IEntity paint, IEntity tank) => Entity(paint.TemplateAccessor!.ConfigPath,
        builder =>
            builder
                .AddComponent<TankPaintBattleItemComponent>()
                .AddComponentFrom<UserGroupComponent>(tank)
                .AddComponentFrom<BattleGroupComponent>(tank)
                .AddComponentFrom<TankGroupComponent>(tank)
                .AddComponentFrom<MarketItemGroupComponent>(paint));
}