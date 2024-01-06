namespace Vint.Core.StateMachine;

public abstract class StateManager {
    public abstract State CurrentState { get; protected set; }

    public virtual void Tick() {
        if (!CurrentState.Finished) 
            CurrentState.Tick();
    }

    public virtual void SetState(State state) {
        CurrentState.Finish();
        state.Start();
        CurrentState = state;
    }
}