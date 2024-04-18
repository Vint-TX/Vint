namespace Vint.Core.StateMachine;

public abstract class State {
    public abstract IStateManager StateManager { get; }
    public bool IsFinished { get; protected set; }

    public virtual void Start() => IsFinished = false;
    
    public virtual void Started() { }

    public virtual void Tick() { }

    public virtual void Finish() => IsFinished = true;
    
    public virtual void Finished() { }
    
    public override string ToString() => GetType().Name;
}