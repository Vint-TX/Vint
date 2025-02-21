using Serilog;
using Vint.Core.Utils;

namespace Vint.Core.StateMachine;

public interface IStateManager;

public abstract class StateManager<T> : IStateManager where T : State {
    protected StateManager() =>
        Logger = Log.Logger.ForType(GetType());

    ILogger Logger { get; }

    public T CurrentState { get; private set; } = null!;

    public abstract Task Init();

    protected async Task InitState(T state) {
        await state.Start();
        CurrentState = state;
        await state.Started();
    }

    public virtual async Task SetState(T state) {
        Logger.Debug("Set state from {Current} to {Next}", CurrentState, state);

        T prevState = CurrentState;

        await prevState.Finish();
        await state.Start();
        CurrentState = state;
        await state.Started();
        await prevState.Finished();
    }

    public virtual async Task Tick(TimeSpan deltaTime) {
        if (CurrentState != null! && !CurrentState.IsFinished)
            await CurrentState.Tick(deltaTime);
    }

    public override string ToString() => $"{GetType().Name}: {CurrentState}";
}
