using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle;

[ProtocolId(140335313420508312)]
public class RoundUserTemplate : EntityTemplate {
    public IEntity Create(BattlePlayer battlePlayer, IEntity tank) {
        IEntity entity = Entity("battle/round/rounduser",
            builder =>
                builder
                    .AddComponent(new RoundUserComponent())
                    .AddComponent(tank.GetComponent<UserGroupComponent>())
                    .AddComponent(tank.GetComponent<BattleGroupComponent>()));

        if (battlePlayer.Team != null)
            entity.AddComponent(battlePlayer.Team.GetComponent<TeamGroupComponent>());

        entity.AddComponent(new RoundUserStatisticsComponent(0, 0, 0, 0, 0));

        return entity;
    }
}