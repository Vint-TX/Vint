using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Components.Battle.Limit;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Battle.Time;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Events.Battle;
using Vint.Core.Server.Game;
using Vint.Core.StateMachine;
using Vint.Core.Utils;

namespace Vint.Core.Battles.States;

public abstract class BattleState(
    BattleStateManager stateManager
) : State {
    public override BattleStateManager StateManager { get; } = stateManager;
    protected Battle Battle => StateManager.Battle;
}

public class NotEnoughPlayers(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override async Task Tick(TimeSpan deltaTime) {
        if (Battle.Players.Count > 0)
            await StateManager.SetState(new Countdown(StateManager));

        await base.Tick(deltaTime);
    }
}

public class NotStarted(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override async Task Start() {
        Battle.Timer = TimeSpan.Zero;
        await base.Start();
    }
}

public class Countdown(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override async Task Start() {
        TimeSpan startDuration = TimeSpan.FromSeconds(20);

        await Battle.LobbyEntity.AddComponent(new MatchmakingLobbyStartTimeComponent(DateTimeOffset.UtcNow + startDuration));
        Battle.Timer = startDuration;
        await base.Start();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        if (Battle.Players.Count <= 0)
            await StateManager.SetState(new NotEnoughPlayers(StateManager));
        else if (Battle.Timer < TimeSpan.Zero)
            await StateManager.SetState(new Starting(StateManager));

        await base.Tick(deltaTime);
    }

    public override async Task Finish() {
        await Battle.LobbyEntity.RemoveComponent<MatchmakingLobbyStartTimeComponent>();
        await base.Finish();
    }
}

public class Starting(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override async Task Start() {
        await Battle.LobbyEntity.AddComponent<MatchmakingLobbyStartingComponent>();
        await base.Start();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        switch (Battle.TypeHandler) {
            case MatchmakingHandler:
                await MatchmakingBattleTick();
                break;

            case ArcadeHandler:
                await ArcadeBattleTick();
                break;

            case CustomHandler:
                await CustomBattleTick();
                break;
        }

        await base.Tick(deltaTime);
    }

    async Task MatchmakingBattleTick() {
        if (Battle.Players.Count <= 0) {
            await StateManager.SetState(new NotEnoughPlayers(StateManager));
        } else if (Battle.Timer < TimeSpan.Zero) {
            await Battle.Start();
            await StateManager.SetState(new WarmUp(StateManager));
        }
    }

    async Task ArcadeBattleTick() {
        if (Battle.Players.Count <= 0) {
            await StateManager.SetState(new NotEnoughPlayers(StateManager));
        } else if (Battle.Timer < TimeSpan.Zero) {
            await Battle.Start();
            await StateManager.SetState(new Running(StateManager));
        }
    }

    async Task CustomBattleTick() {
        if (Battle.Players.Count == 0)
            await StateManager.SetState(new NotStarted(StateManager));
        else if (Battle.Timer < TimeSpan.Zero) {
            await Battle.Start();
            await Battle.LobbyEntity.AddGroupComponent<BattleGroupComponent>(Battle.Entity);
            await StateManager.SetState(new Running(StateManager));
        }
    }

    public override async Task Finish() {
        await Battle.LobbyEntity.RemoveComponent<MatchmakingLobbyStartingComponent>();
        await base.Finish();
    }
}

public class WarmUp(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public WarmUpStateManager WarmUpStateManager { get; } = new(stateManager);

    public override async Task Start() {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        TimeSpan warmUp = TimeSpan.FromSeconds(Battle.Entity.GetComponent<TimeLimitComponent>().WarmingUpTimeLimitSec);

        await Battle.Entity.ChangeComponent<BattleStartTimeComponent>(component =>
            component.RoundStartTime = now + warmUp);

        await Battle.RoundEntity.ChangeComponent<RoundStopTimeComponent>(component =>
            component.StopTime = now.AddMinutes(Battle.Properties.TimeLimit));

        foreach (IPlayerConnection connection in Battle.Players.Where(player => player.InBattle).Select(player => player.PlayerConnection))
            await connection.Send(new BattleTimerUpdatedEvent(), Battle.Entity, Battle.RoundEntity);

        await Battle.RoundEntity.AddComponent<RoundWarmingUpStateComponent>();
        Battle.Timer = warmUp;
        await base.Start();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await WarmUpStateManager.Tick(deltaTime);
        await base.Tick(deltaTime);
    }

    public override async Task Finish() {
        await Battle.RoundEntity.RemoveComponent<RoundWarmingUpStateComponent>();
        await base.Finish();
        await Battle.ModeHandler.OnWarmUpCompleted();
    }
}

public class Running(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override async Task Start() {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Battle.Timer = TimeSpan.FromMinutes(Battle.Properties.TimeLimit);

        await Battle.Entity.ChangeComponent<BattleStartTimeComponent>(component =>
            component.RoundStartTime = now);

        await Battle.RoundEntity.ChangeComponent<RoundStopTimeComponent>(component =>
            component.StopTime = now.AddMinutes(Battle.Properties.TimeLimit));
        await base.Start();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        switch (Battle.TypeHandler) {
            case CustomHandler:
                await CustomBattleTick();
                break;

            case ArcadeHandler:
            case MatchmakingHandler:
                await NonCustomBattleTick();
                break;
        }

        if (Battle.Timer < TimeSpan.Zero)
            await Battle.Finish();

        await base.Tick(deltaTime);
    }

    async Task NonCustomBattleTick() {
        if (Battle.ModeHandler is not TeamHandler teamHandler) return;

        if (Battle is { DominationCanBegin: true }) {
            TeamColor dominatedTeam = teamHandler.GetDominatedTeam();

            if (dominatedTeam == TeamColor.None) return;

            Battle.DominationStartTime = DateTimeOffset.UtcNow;
            Battle.StopTimeComponentBeforeDomination = Battle.RoundEntity.GetComponent<RoundStopTimeComponent>().Clone();
            DateTimeOffset battleEndTime = Battle.DominationStartTime.Value + Battle.DominationDuration;

            await Battle.RoundEntity.AddComponent(new RoundDisbalancedComponent(dominatedTeam,
                Convert.ToInt32(Battle.DominationDuration.TotalSeconds),
                battleEndTime));

            await Battle.RoundEntity.ChangeComponent<RoundStopTimeComponent>(component => component.StopTime = battleEndTime);
        } else if (Battle.DominationStartTime.HasValue) {
            TeamColor dominatedTeam = teamHandler.GetDominatedTeam();

            if (dominatedTeam != TeamColor.None) {
                if (Battle.DominationStartTime.Value + Battle.DominationDuration > DateTimeOffset.UtcNow) return;

                Battle.DominationStartTime = null;
                await Battle.Finish();
                return;
            }

            Battle.DominationStartTime = null;
            await Battle.RoundEntity.ChangeComponent(Battle.StopTimeComponentBeforeDomination!);
            Battle.StopTimeComponentBeforeDomination = null;

            foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattle))
                await battlePlayer.PlayerConnection.Send(new RoundBalanceRestoredEvent(), Battle.RoundEntity);

            await Battle.RoundEntity.RemoveComponent<RoundDisbalancedComponent>();
        }
    }

    async Task CustomBattleTick() {
        if (Battle.Players.Any(player => player.InBattleAsTank)) return;

        await Battle.LobbyEntity.RemoveComponent<BattleGroupComponent>();
        await StateManager.SetState(new Ended(StateManager));
    }
}

public class Ended(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override async Task Start() {
        Battle.Timer = TimeSpan.Zero;
        await base.Start();
    }

    public override async Task Finish() {
        await base.Finish();

        if (Battle.TypeHandler is not CustomHandler) return;

        ModeHandler prevHandler = Battle.ModeHandler;
        Battle.Setup();
        Battle.ModeHandler.TransferParameters(prevHandler);
    }
}
