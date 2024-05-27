namespace Vint.Core.StateMachine;

public abstract class State {
    public abstract IStateManager StateManager { get; }
    public bool IsFinished { get; protected set; }

    public virtual Task Start() => Task.FromResult(IsFinished = false);

    public virtual Task Started() => Task.CompletedTask;

    public virtual Task Tick() => Task.CompletedTask;

    public virtual Task Finish() => Task.FromResult(IsFinished = true);

    public virtual Task Finished() => Task.CompletedTask;

    public override string ToString() => GetType().Name;
}
