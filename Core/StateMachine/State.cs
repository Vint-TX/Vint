namespace Vint.Core.StateMachine;

public abstract class State {
    public abstract StateManager StateManager { get; }
    public bool Finished { get; protected set; }

    public virtual void Start() => Finished = false;
    
    public virtual void Tick() { }

    public virtual void Finish() => Finished = true;
}