using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.Battle.Rounds.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Battle.Player.User.Templates;

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
