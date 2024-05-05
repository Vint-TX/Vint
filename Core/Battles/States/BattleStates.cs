using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Type;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Battle.Time;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Events.Battle;
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
    public override async Task Tick() {
        if (Battle.Players.Count > 0)
            StateManager.SetState(new Countdown(StateManager));

        await base.Tick();
    }
}

public class NotStarted(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override async Task Start() {
        Battle.Timer = 0;
        await base.Start();
    }
}

public class Countdown(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override async Task Start() {
        const int seconds = 20;

        Battle.LobbyEntity.AddComponent(new MatchmakingLobbyStartTimeComponent(DateTimeOffset.UtcNow.AddSeconds(seconds)));
        Battle.Timer = seconds;
        await base.Start();
    }

    public override async Task Tick() {
        if (Battle.Players.Count <= 0)
            StateManager.SetState(new NotEnoughPlayers(StateManager));
        else if (Battle.Timer < 0)
            StateManager.SetState(new Starting(StateManager));

        await base.Tick();
    }

    public override void Finish() {
        Battle.LobbyEntity.RemoveComponent<MatchmakingLobbyStartTimeComponent>();
        base.Finish();
    }
}

public class Starting(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override async Task Start() {
        Battle.LobbyEntity.AddComponent<MatchmakingLobbyStartingComponent>();
        await base.Start();
    }

    public override async Task Tick() {
        switch (Battle.TypeHandler) {
            case MatchmakingHandler:
                MatchmakingBattleTick();
                break;

            case ArcadeHandler:
                ArcadeBattleTick();
                break;

            case CustomHandler:
                CustomBattleTick();
                break;
        }

        await base.Tick();
    }

    void MatchmakingBattleTick() {
        if (Battle.Players.Count <= 0) {
            StateManager.SetState(new NotEnoughPlayers(StateManager));
        } else if (Battle.Timer < 0) {
            Battle.Start();
            StateManager.SetState(new WarmUp(StateManager));
        }
    }

    void ArcadeBattleTick() {
        if (Battle.Players.Count <= 0) {
            StateManager.SetState(new NotEnoughPlayers(StateManager));
        } else if (Battle.Timer < 0) {
            Battle.Start();
            StateManager.SetState(new Running(StateManager));
        }
    }

    void CustomBattleTick() {
        if (Battle.Players.Count == 0)
            StateManager.SetState(new NotStarted(StateManager));
        else if (Battle.Timer < 0) {
            Battle.Start();
            Battle.LobbyEntity.AddComponentFrom<BattleGroupComponent>(Battle.Entity);
            StateManager.SetState(new Running(StateManager));
        }
    }

    public override void Finish() {
        Battle.LobbyEntity.RemoveComponent<MatchmakingLobbyStartingComponent>();
        base.Finish();
    }
}

public class WarmUp(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public WarmUpStateManager WarmUpStateManager { get; } = new(stateManager);

    public override async Task Start() {
        const int seconds = 60;
        Battle.Entity.ChangeComponent<BattleStartTimeComponent>(component =>
            component.RoundStartTime = DateTimeOffset.UtcNow.AddSeconds(seconds));

        Battle.RoundEntity.ChangeComponent<RoundStopTimeComponent>(component =>
            component.StopTime = DateTimeOffset.UtcNow.AddMinutes(Battle.Properties.TimeLimit));

        Battle.RoundEntity.AddComponent<RoundWarmingUpStateComponent>();
        Battle.Timer = seconds;
        await base.Start();
    }

    public override async Task Tick() {
        WarmUpStateManager.Tick();
        await base.Tick();
    }

    public override void Finish() {
        Battle.RoundEntity.RemoveComponent<RoundWarmingUpStateComponent>();
        base.Finish();
        Battle.ModeHandler.OnWarmUpCompleted();
    }
}

public class Running(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override async Task Start() {
        Battle.Timer = Battle.Properties.TimeLimit * 60;

        Battle.Entity.ChangeComponent<BattleStartTimeComponent>(component =>
            component.RoundStartTime = DateTimeOffset.UtcNow);

        Battle.RoundEntity.ChangeComponent<RoundStopTimeComponent>(component =>
            component.StopTime = DateTimeOffset.UtcNow.AddMinutes(Battle.Properties.TimeLimit));
        await base.Start();
    }

    public override async Task Tick() {
        switch (Battle.TypeHandler) {
            case CustomHandler:
                CustomBattleTick();
                break;

            case ArcadeHandler:
            case MatchmakingHandler:
                await NonCustomBattleTick();
                break;
        }

        if (Battle.Timer < 0)
            await Battle.Finish();

        await base.Tick();
    }

    async Task NonCustomBattleTick() {
        if (Battle.ModeHandler is not TeamHandler teamHandler) return;

        if (Battle is { DominationCanBegin: true }) {
            TeamColor dominatedTeam = teamHandler.GetDominatedTeam();

            if (dominatedTeam == TeamColor.None) return;

            Battle.DominationStartTime = DateTimeOffset.UtcNow;
            Battle.StopTimeComponentBeforeDomination = Battle.RoundEntity.GetComponent<RoundStopTimeComponent>().Clone();
            DateTimeOffset battleEndTime = Battle.DominationStartTime.Value + Battle.DominationDuration;

            Battle.RoundEntity.AddComponent(new RoundDisbalancedComponent(dominatedTeam,
                Convert.ToInt32(Battle.DominationDuration.TotalSeconds),
                battleEndTime));

            Battle.RoundEntity.ChangeComponent<RoundStopTimeComponent>(component => component.StopTime = battleEndTime);
        } else if (Battle.DominationStartTime.HasValue) {
            TeamColor dominatedTeam = teamHandler.GetDominatedTeam();

            if (dominatedTeam != TeamColor.None) {
                if (Battle.DominationStartTime.Value + Battle.DominationDuration > DateTimeOffset.UtcNow) return;

                Battle.DominationStartTime = null;
                await Battle.Finish();
                return;
            }

            Battle.DominationStartTime = null;
            Battle.RoundEntity.ChangeComponent(Battle.StopTimeComponentBeforeDomination!);
            Battle.StopTimeComponentBeforeDomination = null;

            foreach (BattlePlayer battlePlayer in Battle.Players.Where(player => player.InBattle))
                battlePlayer.PlayerConnection.Send(new RoundBalanceRestoredEvent(), Battle.RoundEntity);

            Battle.RoundEntity.RemoveComponent<RoundDisbalancedComponent>();
        }
    }

    void CustomBattleTick() {
        if (Battle.Players.Any(player => player.InBattleAsTank)) return;

        Battle.LobbyEntity.RemoveComponent<BattleGroupComponent>();
        StateManager.SetState(new Ended(StateManager));
    }
}

public class Ended(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override async Task Start() {
        Battle.Timer = 0;
        await base.Start();
    }

    public override void Finish() {
        base.Finish();

        if (Battle.TypeHandler is not CustomHandler) return;

        ModeHandler prevHandler = Battle.ModeHandler;
        Battle.Setup();
        Battle.ModeHandler.TransferParameters(prevHandler);
    }
}
