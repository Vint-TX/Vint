namespace Vint.Core.StateMachine;

public abstract class State {
    public abstract IStateManager StateManager { get; }
    public bool Finished { get; protected set; }

    public virtual void Start() => Finished = false;

    public virtual void Tick() { }

    public virtual void Finish() => Finished = true;

    public override string ToString() => GetType().Name;
}