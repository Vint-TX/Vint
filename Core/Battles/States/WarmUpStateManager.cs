using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public class WarmUpStateManager : StateManager<WarmUpState> {
    public WarmUpStateManager(BattleStateManager battleStateManager) {
        BattleStateManager = battleStateManager;
        CurrentState = new WarmingUp(this);
    }

    public BattleStateManager BattleStateManager { get; }
}