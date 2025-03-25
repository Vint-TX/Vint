using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Items.Components;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Tank.Common.Templates;

[ProtocolId(636047163591561471)]
public class HullSkinBattleItemTemplate : EntityTemplate {
    public IEntity Create(IEntity skin, IEntity tank) => Entity(skin.TemplateAccessor!.ConfigPath,
        builder => builder
            .AddComponent<HullSkinBattleItemComponent>()
            .AddComponentFrom<UserGroupComponent>(tank)
            .AddComponentFrom<BattleGroupComponent>(tank)
            .AddGroupComponent<TankGroupComponent>(tank)
            .AddGroupComponent<MarketItemGroupComponent>(skin));
}
