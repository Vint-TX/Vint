using Vint.Core.Config;
using Vint.Core.ECS.Components.Battle.Parameters.Chassis;
using Vint.Core.ECS.Components.Battle.Parameters.Health;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Tank;

[ProtocolId(2012489519776979402)]
public class TankTemplate : EntityTemplate {
    public IEntity Create(IEntity hull, IEntity battleUser) {
        string configPath = hull.TemplateAccessor!.ConfigPath!;

        IEntity entity = Entity(configPath.Replace("garage", "battle"),
            builder => builder
                .AddComponent(new TankComponent())
                .AddComponent(new TankPartComponent())
                .AddComponent(new TankNewStateComponent())
                .AddComponent(new TemperatureComponent(0))
                .AddComponent(battleUser.GetComponent<UserGroupComponent>())
                .AddComponent(battleUser.GetComponent<BattleGroupComponent>())
                .AddComponent(hull.GetComponent<MarketItemGroupComponent>())
                .AddComponent(ConfigManager.GetComponent<HealthComponent>(configPath))
                .AddComponent(ConfigManager.GetComponent<HealthConfigComponent>(configPath))
                .AddComponent(ConfigManager.GetComponent<DampingComponent>(configPath))
                .AddComponent(ConfigManager.GetComponent<SpeedComponent>(configPath))
                .AddComponent(ConfigManager.GetComponent<SpeedConfigComponent>(configPath))
                .AddComponent(ConfigManager.GetComponent<WeightComponent>(configPath)));

        if (battleUser.HasComponent<TeamGroupComponent>())
            entity.AddComponent(battleUser.GetComponent<TeamGroupComponent>());

        entity.AddComponent(new TankGroupComponent(entity));
        return entity;
    }
}