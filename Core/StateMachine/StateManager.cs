using Serilog;
using Vint.Core.Utils;

namespace Vint.Core.StateMachine;

public interface IStateManager;

public abstract class StateManager<T> : IStateManager where T : State {
    protected StateManager() =>
        Logger = Log.Logger.ForType(GetType());

    ILogger Logger { get; }

    public T CurrentState { get; protected set; } = null!;

    public virtual void Tick() {
        if (!CurrentState.Finished)
            CurrentState.Tick();
    }

    public virtual void SetState(T state) {
        Logger.Debug("Set state from {Current} to {Next}", CurrentState, state);

        CurrentState.Finish();
        state.Start();
        CurrentState = state;
    }

    public override string ToString() => $"{GetType().Name}: {CurrentState}";
}