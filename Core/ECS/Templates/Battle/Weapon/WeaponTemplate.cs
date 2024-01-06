using Vint.Core.Battles.Player;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Weapon;

[ProtocolId(1430285417001)]
public abstract class WeaponTemplate : EntityTemplate {
    protected virtual IEntity Create(string configPath, IEntity tank, BattlePlayer battlePlayer) {
        IEntity entity = Entity(configPath.Replace("garage", "battle"),
            builder => builder
                .AddComponent(new WeaponComponent())
                .AddComponent(tank.GetComponent<TankPartComponent>())
                .AddComponent(tank.GetComponent<UserGroupComponent>())
                .AddComponent(tank.GetComponent<TankGroupComponent>())
                .AddComponent(tank.GetComponent<BattleGroupComponent>())
                .AddComponent(battlePlayer.PlayerConnection.Player.CurrentPreset.Weapon.GetComponent<MarketItemGroupComponent>())
                .AddComponent(ConfigManager.GetComponent<WeaponRotationComponent>(tank.TemplateAccessor!.ConfigPath!.Replace("battle", "garage"))));

        // todo hammer

        if (battlePlayer.Team != null)
            entity.AddComponent(battlePlayer.Team.GetComponent<TeamGroupComponent>());

        if (ConfigManager.TryGetComponent(configPath, out WeaponCooldownComponent? cooldownComponent)) {
            // todo shaft

            entity.AddComponent(cooldownComponent);
        }

        return entity;
    }
}