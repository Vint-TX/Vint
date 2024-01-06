using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(-5342270968507348251)]
public class ShellBattleItemTemplate : EntityTemplate {
    public IEntity Create(IEntity shell, IEntity tank) => Entity(shell.TemplateAccessor!.ConfigPath, builder => 
        builder
            .AddComponent(new ShellBattleItemComponent())
            .AddComponent(tank.GetComponent<UserGroupComponent>())
            .AddComponent(tank.GetComponent<BattleGroupComponent>())
            .AddComponent(tank.GetComponent<TankGroupComponent>())
            .AddComponent(shell.GetComponent<MarketItemGroupComponent>()));
}