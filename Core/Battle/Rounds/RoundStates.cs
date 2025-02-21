using Vint.Core.Battle.Player;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Components.Battle.Round;
using Vint.Core.StateMachine;

namespace Vint.Core.Battle.Rounds;

public class RoundStateManager(
    Round round,
    Func<RoundStateManager, RoundState> initStateFactory
) : StateManager<RoundState> {
    public Round Round { get; } = round;

    Func<RoundStateManager, RoundState> InitStateFactory { get; } = initStateFactory;

    public override async Task Init() =>
        await InitState(InitStateFactory(this));
}

public abstract class RoundState(
    RoundStateManager stateManager
) : State {
    public override RoundStateManager StateManager { get; } = stateManager;
    public Round Round => StateManager.Round;
}

public class WarmUp(
    RoundStateManager stateManager
) : RoundState(stateManager) {
    static IEnumerable<StateTransition> TransitionsTemplate { get; } = [
        new(TimeSpan.FromSeconds(11), WarmUpState.PrepareToFight),
        new(TimeSpan.FromSeconds(6), WarmUpState.MatchBeginsIn),
        new(TimeSpan.FromSeconds(4.5), WarmUpState.Three),
        new(TimeSpan.FromSeconds(3.25), WarmUpState.Two),
        new(TimeSpan.FromSeconds(2), WarmUpState.One),
        new(TimeSpan.FromSeconds(0), WarmUpState.LetsRoll)
    ];

    Queue<StateTransition> Transitions { get; } = new(TransitionsTemplate);
    TimeSpan? NextRemainingTime { get; set; }

    public WarmUpState CurrentState { get; private set; } = WarmUpState.WarmingUp;
    public bool IsPreparing { get; private set; }

    public override async Task Start() {
        NextRemainingTime = GetNextRemainingTime();
        await Round.Entity.AddComponent<RoundWarmingUpStateComponent>();
        await base.Start();
    }

    public override async Task Finish() {
        await Round.Entity.RemoveComponent<RoundWarmingUpStateComponent>();
        await Round.OnWarmUpEnded();
        await base.Finish();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        if (TryChangeState()) {
            switch (CurrentState) {
                case WarmUpState.PrepareToFight:
                    IsPreparing = true;
                    break;

                case WarmUpState.MatchBeginsIn:
                    await ResetTanks();
                    break;

                case WarmUpState.LetsRoll:
                    await SpawnTanks();
                    await StateManager.SetState(new Running(StateManager));
                    break;
            }
        }
    }

    async Task ResetTanks() {
        foreach (Tanker tanker in Round.Tankers)
            await tanker.Tank.ResetAndDisable();
    }

    async Task SpawnTanks() {
        foreach (Tanker tanker in Round.Tankers) {
            TankStateManager stateManager = tanker.Tank.StateManager;
            await stateManager.SetState(new Spawn(stateManager));
        }
    }

    bool TryChangeState() {
        if (Transitions.Count == 0 || NextRemainingTime < Round.RemainingWarmUp)
            return false;

        StateTransition transition = Transitions.Dequeue();
        CurrentState = transition.State;
        NextRemainingTime = GetNextRemainingTime();
        return true;
    }

    TimeSpan? GetNextRemainingTime() => Transitions.Count > 0 ? Transitions.Peek().RemainingTime : null;

    public enum WarmUpState : byte {
        WarmingUp,
        PrepareToFight,
        MatchBeginsIn,
        Three,
        Two,
        One,
        LetsRoll
    }

    readonly record struct StateTransition(
        TimeSpan RemainingTime,
        WarmUpState State
    );
}

public class Running(
    RoundStateManager stateManager
) : RoundState(stateManager) {
    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        if (Round.Remaining <= TimeSpan.Zero)
            await Round.End();
    }
}

public class Ended(
    RoundStateManager stateManager
) : RoundState(stateManager) {
    public bool WasEnemies { get; private set; }

    public override async Task Start() {
        WasEnemies = Round.ModeHandler.HasEnemies;
        await base.Start();
    }
}
