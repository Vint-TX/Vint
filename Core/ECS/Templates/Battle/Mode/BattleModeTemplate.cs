using Vint.Core.Battles;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Limit;
using Vint.Core.ECS.Components.Battle.Time;
using Vint.Core.ECS.Components.Battle.Type;
using Vint.Core.ECS.Components.Battle.User;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;

namespace Vint.Core.ECS.Templates.Battle.Mode;

public abstract class BattleModeTemplate : EntityTemplate {
    protected IEntity Entity(
        TypeHandler typeHandler,
        IEntity lobby,
        BattleMode mode,
        TimeSpan timeLimit,
        int userLimit,
        TimeSpan warmUpTimeLimit) => Entity(
        $"battle/modes/{mode.ToString().ToLower()}",
        builder => builder
            .AddComponent<BattleComponent>()
            .AddComponent<BattleConfiguredComponent>()
            .AddComponent<BattleTankCollisionsComponent>()
            .AddComponent<VisibleItemComponent>()
            .AddComponent(new UserCountComponent(userLimit))
            .AddComponent(new TimeLimitComponent(timeLimit, warmUpTimeLimit))
            .AddComponent(new BattleStartTimeComponent(DateTimeOffset.UtcNow + warmUpTimeLimit))
            .AddComponentFrom<GravityComponent>(lobby)
            .AddComponentFrom<BattleModeComponent>(lobby)
            .AddComponentFrom<UserLimitComponent>(lobby)
            .AddComponentFrom<MapGroupComponent>(lobby)
            .AddGroupComponent<BattleLobbyGroupComponent>(lobby)
            .AddGroupComponent<BattleGroupComponent>()
            .ThenExecuteIf(_ => typeHandler is ArcadeHandler, entity => entity.AddComponent<ArcadeBattleComponent>())
            .ThenExecuteIf(_ => typeHandler is MatchmakingHandler, entity => entity.AddComponent<RatingBattleComponent>())
            .ThenExecuteIf(_ => typeHandler is CustomHandler, entity => entity.AddComponent<CustomBattleComponent>()));

    public abstract IEntity Create(TypeHandler typeHandler, IEntity lobby, TimeSpan timeLimit, int userLimit, TimeSpan warmUpTimeLimit);
}
