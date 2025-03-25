using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.Battle.Tank.Incarnation.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Tank.Incarnation.Templates;

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
