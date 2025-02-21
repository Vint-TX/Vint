using Vint.Core.ECS.Components.Battle.Incarnation;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Incarnation;

[ProtocolId(1478091203635)]
public class TankIncarnationTemplate : EntityTemplate {
    public IEntity Create(IEntity tank, IEntity user, IEntity? team) => Entity(null,
        builder => builder
            .AddComponent<TankIncarnationComponent>()
            .AddComponent(new TankIncarnationKillStatisticsComponent(0))
            .AddGroupComponent<TankGroupComponent>(tank)
            .AddGroupComponent<UserGroupComponent>(user)
            .ThenExecuteIf(_ => team != null,
                entity => entity.AddGroupComponent<TeamGroupComponent>(team)));
}
