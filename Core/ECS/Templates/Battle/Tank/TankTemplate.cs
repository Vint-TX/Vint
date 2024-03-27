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

        return Entity(configPath.Replace("garage", "battle"),
            builder => builder
                .AddComponent<TankComponent>()
                .AddComponent<TankPartComponent>()
                .AddComponent<TankNewStateComponent>()
                .AddComponent(new TemperatureComponent(0))
                .AddComponent<HealthComponent>(configPath)
                .AddComponent<HealthConfigComponent>(configPath)
                .AddComponent<DampingComponent>(configPath)
                .AddComponent<SpeedComponent>(configPath)
                .AddComponent<SpeedConfigComponent>(configPath)
                .AddComponent<WeightComponent>(configPath)
                .AddComponentFrom<UserGroupComponent>(battleUser)
                .AddComponentFrom<BattleGroupComponent>(battleUser)
                .AddComponentFrom<MarketItemGroupComponent>(hull)
                .AddGroupComponent<TankGroupComponent>()
                .ThenExecuteIf(_ => battleUser.HasComponent<TeamGroupComponent>(),
                    entity => entity.AddComponentFrom<TeamGroupComponent>(battleUser)));
    }
}