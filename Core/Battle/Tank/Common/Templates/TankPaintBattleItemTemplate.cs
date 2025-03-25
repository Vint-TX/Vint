using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Items.Components;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Tank.Common.Templates;

[ProtocolId(1437375358285)]
public class TankPaintBattleItemTemplate : EntityTemplate {
    public IEntity Create(IEntity paint, IEntity tank) => Entity(paint.TemplateAccessor!.ConfigPath,
        builder => builder
            .AddComponent<TankPaintBattleItemComponent>()
            .AddComponentFrom<UserGroupComponent>(tank)
            .AddComponentFrom<BattleGroupComponent>(tank)
            .AddGroupComponent<TankGroupComponent>(tank)
            .AddGroupComponent<MarketItemGroupComponent>(paint));
}
