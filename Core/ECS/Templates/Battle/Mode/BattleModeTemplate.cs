using Vint.Core.Battles;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Limit;
using Vint.Core.ECS.Components.Battle.Time;
using Vint.Core.ECS.Components.Battle.User;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;

namespace Vint.Core.ECS.Templates.Battle.Mode;

public abstract class BattleModeTemplate : EntityTemplate {
    protected IEntity Entity(IEntity lobby, BattleMode mode, int scoreLimit, int timeLimit, int warmUpTimeLimit) {
        UserLimitComponent userLimitComponent = lobby.GetComponent<UserLimitComponent>();

        IEntity entity = Entity($"battle/modes/{mode.ToString().ToLower()}",
            builder => {
                builder.AddComponent(userLimitComponent)
                    .AddComponent(lobby.GetComponent<MapGroupComponent>())
                    .AddComponent(lobby.GetComponent<GravityComponent>())
                    .AddComponent(lobby.GetComponent<BattleModeComponent>())
                    .AddComponent(new BattleComponent())
                    .AddComponent(new BattleConfiguredComponent())
                    .AddComponent(new BattleTankCollisionsComponent())
                    .AddComponent(new VisibleItemComponent())
                    .AddComponent(new UserCountComponent(userLimitComponent.UserLimit))
                    .AddComponent(new TimeLimitComponent(timeLimit, warmUpTimeLimit))
                    .AddComponent(new ScoreLimitComponent(scoreLimit))
                    .AddComponent(new BattleStartTimeComponent(DateTimeOffset.UtcNow));
            });

        entity.AddComponent(new BattleGroupComponent(entity));
        return entity;
    }

    public abstract IEntity Create(IEntity lobby, int scoreLimit, int timeLimit, int warmUpTimeLimit);
}