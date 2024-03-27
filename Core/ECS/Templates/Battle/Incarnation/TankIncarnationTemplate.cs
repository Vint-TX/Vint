using Vint.Core.ECS.Components.Battle.Incarnation;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Incarnation;

[ProtocolId(1478091203635)]
public class TankIncarnationTemplate : EntityTemplate {
    public IEntity Create(IEntity tank) => Entity(null,
        builder =>
            builder
                .AddComponent<TankIncarnationComponent>()
                .AddComponent(new TankIncarnationKillStatisticsComponent(0))
                .AddComponentFrom<TankGroupComponent>(tank));
}