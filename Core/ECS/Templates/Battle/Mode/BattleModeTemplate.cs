using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Limit;
using Vint.Core.ECS.Components.Battle.Time;
using Vint.Core.ECS.Components.Battle.Type;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Lobby;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;

namespace Vint.Core.ECS.Templates.Battle.Mode;

public abstract class BattleModeTemplate : EntityTemplate {
    protected IEntity Entity(BattleProperties properties, IEntity lobby, IEntity round, DateTimeOffset startTime) {
        BattleMode mode = properties.BattleMode;
        BattleType type = properties.Type;
        TimeSpan warmUpDuration = properties.WarmUpDuration;
        int timeLimitSec = properties.TimeLimit * 60;

        return Entity($"battle/modes/{mode.ToString().ToLower()}",
            builder => builder
                .AddComponent<BattleComponent>()
                .AddComponent<BattleTankCollisionsComponent>()
                .AddComponent(new TimeLimitComponent(timeLimitSec, (int)warmUpDuration.TotalSeconds))
                .AddComponent(new BattleStartTimeComponent(startTime))
                .AddComponentFrom<GravityComponent>(lobby)
                .AddComponentFrom<BattleModeComponent>(lobby)
                .AddComponentFrom<UserLimitComponent>(lobby)
                .AddComponentFrom<MapGroupComponent>(lobby)
                .AddGroupComponent<BattleLobbyGroupComponent>(lobby)
                .AddGroupComponent<BattleGroupComponent>(round)
                .ThenExecuteIf(_ => type == BattleType.Arcade, entity => entity.AddComponent<ArcadeBattleComponent>())
                .ThenExecuteIf(_ => type == BattleType.Rating, entity => entity.AddComponent<RatingBattleComponent>())
                .ThenExecuteIf(_ => type == BattleType.Custom, entity => entity.AddComponent<CustomBattleComponent>()));
    }

    public abstract IEntity Create(
        BattleProperties properties,
        IEntity lobby,
        IEntity round,
        DateTimeOffset startTime);
}
