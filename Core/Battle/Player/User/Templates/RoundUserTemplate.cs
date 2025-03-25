using Vint.Core.Battle.Player;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle;

[ProtocolId(140335313420508312)]
public class RoundUserTemplate : EntityTemplate {
    public IEntity Create(Tanker tanker, IEntity tank) => Entity("battle/round/rounduser",
        builder => builder
            .AddComponent<RoundUserComponent>()
            .AddComponent<RoundUserStatisticsComponent>()
            .AddComponentFrom<UserGroupComponent>(tank)
            .AddComponentFrom<BattleGroupComponent>(tank)
            .ThenExecuteIf(_ => tanker.Team != null, entity => entity.AddGroupComponent<TeamGroupComponent>(tanker.Team)));
}
