using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.ECS.Components.Battle.Time;
using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Matchmaking;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public abstract class BattleState(
    BattleStateManager stateManager
) : State {
    public override BattleStateManager StateManager { get; } = stateManager;
    public Battle Battle => StateManager.Battle;
}

public class NotEnoughPlayers(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override void Tick() {
        if (Battle.Players.Count > 0)
            StateManager.SetState(new Countdown(StateManager));

        base.Tick();
    }
}

public class NotStarted(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override void Start() {
        Battle.Timer = 0;
        base.Start();
    }
}

public class Countdown(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override void Start() {
        const int seconds = 10;

        Battle.LobbyEntity.AddComponent(new MatchmakingLobbyStartTimeComponent(DateTimeOffset.UtcNow.AddSeconds(seconds)));
        Battle.Timer = seconds;
        base.Start();
    }

    public override void Tick() {
        if (Battle.Players.Count <= 0)
            StateManager.SetState(new NotEnoughPlayers(StateManager));
        else if (Battle.Timer < 0)
            StateManager.SetState(new Starting(StateManager));

        base.Tick();
    }

    public override void Finish() {
        Battle.LobbyEntity.RemoveComponent<MatchmakingLobbyStartTimeComponent>();
        base.Finish();
    }
}

public class Starting(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override void Start() {
        Battle.LobbyEntity.AddComponent(new MatchmakingLobbyStartingComponent());
        base.Start();
    }

    public override void Tick() {
        if (Battle.IsCustom)
            CustomBattleTick();
        else
            MatchmakingBattleTick();

        base.Tick();
    }

    public override void Finish() {
        Battle.LobbyEntity.RemoveComponent<MatchmakingLobbyStartingComponent>();
        base.Finish();
    }

    void CustomBattleTick() {
        if (Battle.Players.Count == 0)
            StateManager.SetState(new NotStarted(StateManager));
        else if (Battle.Timer < 0) {
            Battle.Start();
            Battle.LobbyEntity.AddComponent(Battle.Entity.GetComponent<BattleGroupComponent>());
            StateManager.SetState(new Running(StateManager));
        }
    }

    void MatchmakingBattleTick() {
        if (Battle.Players.Count <= 0) {
            StateManager.SetState(new NotEnoughPlayers(StateManager));
        } else if (Battle.Timer < 0) {
            Battle.Start();
            StateManager.SetState(new WarmUp(StateManager));
        }
    }
}

public class WarmUp(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    WarmUpStateManager WarmUpStateManager { get; } = new(stateManager);

    public override void Start() {
        const int seconds = 60;
        Battle.Entity.ChangeComponent<BattleStartTimeComponent>(component =>
            component.RoundStartTime = DateTimeOffset.UtcNow.AddSeconds(seconds));

        Battle.RoundEntity.ChangeComponent<RoundStopTimeComponent>(component =>
            component.StopTime = DateTimeOffset.UtcNow.AddMinutes(Battle.Properties.TimeLimit));

        Battle.RoundEntity.AddComponent(new RoundWarmingUpStateComponent());
        Battle.Timer = seconds;
        base.Start();
    }

    public override void Tick() {
        WarmUpStateManager.Tick();
        base.Tick();
    }

    public override void Finish() {
        Battle.RoundEntity.RemoveComponent<RoundWarmingUpStateComponent>();
        base.Finish();
        Battle.ModeHandler.OnWarmUpCompleted();
    }
}

public class MatchBegins(
    BattleStateManager stateManager
) : BattleState(stateManager);

public class Running(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override void Start() {
        Battle.Timer = Battle.Properties.TimeLimit * 60;

        Battle.Entity.ChangeComponent<BattleStartTimeComponent>(component =>
            component.RoundStartTime = DateTimeOffset.UtcNow);

        Battle.RoundEntity.ChangeComponent<RoundStopTimeComponent>(component =>
            component.StopTime = DateTimeOffset.UtcNow.AddMinutes(Battle.Properties.TimeLimit));
        base.Start();
    }

    public override void Tick() {
        if (Battle.IsCustom && Battle.Players.All(player => !player.InBattleAsTank)) {
            Battle.LobbyEntity.RemoveComponent<BattleGroupComponent>();
            StateManager.SetState(new Ended(StateManager));
        }

        if (Battle.Timer < 0)
            Battle.Finish();

        base.Tick();
    }
}

public class Ended(
    BattleStateManager stateManager
) : BattleState(stateManager) {
    public override void Start() {
        Battle.Timer = 0;
        base.Start();
    }
}