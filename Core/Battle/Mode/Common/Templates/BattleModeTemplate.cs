using Vint.Core.Battle.Common.Components;
using Vint.Core.Battle.Lobby.Components;
using Vint.Core.Battle.Mode.Common.Components;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Properties.Components;
using Vint.Core.Battle.Rounds.Components;
using Vint.Core.Battle.Rounds.Type.Components;
using Vint.Core.Battle.Tank.Common.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Maps.Components;

namespace Vint.Core.Battle.Mode.Common.Templates;

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
                .AddComponent(new RoundStartTimeComponent(startTime))
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
