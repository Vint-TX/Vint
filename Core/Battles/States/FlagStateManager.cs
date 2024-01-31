using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public class FlagStateManager : StateManager<FlagState> {
    public FlagStateManager(Flag flag) {
        Flag = flag;
        CurrentState = new OnPedestal(this);
    }

    public Flag Flag { get; }
}