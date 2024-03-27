using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(1430285417001)]
public abstract class WeaponTemplate : EntityTemplate {
    protected virtual IEntity Create(string configPath, IEntity tank, BattlePlayer battlePlayer) => Entity(configPath.Replace("garage", "battle"),
        builder => builder
            .AddComponent<WeaponComponent>()
            .AddComponent<WeaponRotationComponent>(tank.TemplateAccessor!.ConfigPath!.Replace("battle", "garage"))
            .TryAddComponent<WeaponCooldownComponent>(configPath)
            .AddComponentFrom<TankPartComponent>(tank)
            .AddComponentFrom<UserGroupComponent>(tank)
            .AddComponentFrom<TankGroupComponent>(tank)
            .AddComponentFrom<BattleGroupComponent>(tank)
            .AddComponentFrom<MarketItemGroupComponent>(battlePlayer.PlayerConnection.Player.CurrentPreset.Weapon)
            .ThenExecuteIf(_ => this is not HammerBattleItemTemplate, entity => entity.AddComponent(new WeaponEnergyComponent(1)))
            .ThenExecuteIf(_ => battlePlayer.Team != null, entity => entity.AddComponentFrom<TeamGroupComponent>(battlePlayer.Team!)));
}