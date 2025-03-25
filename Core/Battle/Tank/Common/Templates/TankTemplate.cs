using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.Battle.Tank.Parameters.Components;
using Vint.Core.Battle.Tank.Temperature.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Items.Components;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Tank.Common.Templates;

[ProtocolId(2012489519776979402)]
public class TankTemplate : EntityTemplate {
    public IEntity Create(IEntity hull, IEntity battleUser) {
        string configPath = hull.TemplateAccessor!.ConfigPath!;

        return Entity(configPath.Replace("garage", "battle"),
            builder => builder
                .AddComponent<TankComponent>()
                .AddComponent<TankPartComponent>()
                .AddComponent<TemperatureComponent>()
                .AddComponent<HealthComponent>(configPath)
                .AddComponent<HealthConfigComponent>(configPath)
                .AddComponent<DampingComponent>(configPath)
                .AddComponent<SpeedComponent>(configPath)
                .AddComponent<SpeedConfigComponent>(configPath)
                .AddComponent<WeightComponent>(configPath)
                .AddComponentFrom<UserGroupComponent>(battleUser)
                .AddComponentFrom<BattleGroupComponent>(battleUser)
                .AddGroupComponent<MarketItemGroupComponent>(hull)
                .AddGroupComponent<TankGroupComponent>()
                .ThenExecuteIf(_ => battleUser.HasComponent<TeamGroupComponent>(),
                    entity => entity.AddComponentFrom<TeamGroupComponent>(battleUser)));
    }
}
