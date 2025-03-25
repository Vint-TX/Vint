using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.Battle.Weapons.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Items.Components;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Weapons.Templates;

[ProtocolId(636287143924344191)]
public class WeaponPaintBattleItemTemplate : EntityTemplate {
    public IEntity Create(IEntity cover, IEntity tank) => Entity(cover.TemplateAccessor!.ConfigPath,
        builder => builder
            .AddComponent<WeaponPaintBattleItemComponent>()
            .AddComponentFrom<UserGroupComponent>(tank)
            .AddComponentFrom<BattleGroupComponent>(tank)
            .AddGroupComponent<TankGroupComponent>(tank)
            .AddGroupComponent<MarketItemGroupComponent>(cover));
}
