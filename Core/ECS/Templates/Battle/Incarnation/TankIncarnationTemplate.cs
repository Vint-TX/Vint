using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Incarnation;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Incarnation;

[ProtocolId(1478091203635)]
public class TankIncarnationTemplate : EntityTemplate {
    public IEntity Create(BattleTank battleTank) => Entity(null,
        builder => builder
            .AddComponent<TankIncarnationComponent>()
            .AddComponent(new TankIncarnationKillStatisticsComponent(0))
            .AddGroupComponent<TankGroupComponent>(battleTank.Tank)
            .AddGroupComponent<UserGroupComponent>(battleTank.BattlePlayer.PlayerConnection.User)
            .ThenExecuteIf(_ => battleTank.BattlePlayer.Team != null,
                entity => entity.AddGroupComponent<TeamGroupComponent>(battleTank.BattlePlayer.Team)));
}