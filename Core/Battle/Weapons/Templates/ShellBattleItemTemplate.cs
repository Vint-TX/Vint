using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.Battle.Weapons.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Items.Components;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Weapons.Templates;

[ProtocolId(-5342270968507348251)]
public class ShellBattleItemTemplate : EntityTemplate {
    public IEntity Create(IEntity shell, IEntity tank) => Entity(shell.TemplateAccessor!.ConfigPath,
        builder => builder
            .AddComponent<ShellBattleItemComponent>()
            .AddComponentFrom<UserGroupComponent>(tank)
            .AddComponentFrom<BattleGroupComponent>(tank)
            .AddGroupComponent<TankGroupComponent>(tank)
            .AddGroupComponent<MarketItemGroupComponent>(shell));
}
