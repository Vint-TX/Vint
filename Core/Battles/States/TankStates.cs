using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public abstract class TankState(
    TankStateManager stateManager
) : State {
    public abstract DateTimeOffset TimeToNextState { get; }
    public abstract IComponent StateComponent { get; }
    public override TankStateManager StateManager { get; } = stateManager;
    public BattleTank BattleTank => StateManager.BattleTank;

    public override void Start() {
        BattleTank.Tank.AddComponent(StateComponent);
        base.Start();
    }

    public override void Finish() {
        if (BattleTank.Tank.HasComponent(StateComponent))
            BattleTank.Tank.RemoveComponent(StateComponent);
        base.Finish();
    }
}

public class New(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override DateTimeOffset TimeToNextState { get; } = DateTimeOffset.UtcNow;
    public override IComponent StateComponent { get; } = new TankNewStateComponent();
}

public class Dead(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override DateTimeOffset TimeToNextState { get; } = DateTimeOffset.UtcNow.AddSeconds(3);
    public override IComponent StateComponent { get; } = new TankDeadStateComponent();

    public override void Start() { // todo
        BattleTank.Disable();
        base.Start();
    }

    public override void Tick() {
        if (DateTimeOffset.UtcNow >= TimeToNextState)
            StateManager.SetState(new Spawn(StateManager));
    }
}

public class Spawn(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override DateTimeOffset TimeToNextState { get; } = DateTimeOffset.UtcNow.AddSeconds(1.75);
    public override IComponent StateComponent { get; } = new TankSpawnStateComponent();

    public override void Start() {
        BattleTank.Disable();
        BattleTank.PrepareForRespawning();
        base.Start();
    }

    public override void Tick() {
        if (DateTimeOffset.UtcNow >= TimeToNextState)
            StateManager.SetState(new SemiActive(StateManager));
    }
}

public class SemiActive(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override DateTimeOffset TimeToNextState { get; } = DateTimeOffset.UtcNow.AddSeconds(0.5);
    public override IComponent StateComponent { get; } = new TankSemiActiveStateComponent();

    public override void Start() {
        BattleTank.Enable();
        BattleTank.Tank.ChangeComponent<TemperatureComponent>(component => component.Temperature = 0);
        BattleTank.Tank.AddComponent(new TankVisibleStateComponent());
        base.Start();
    }

    public override void Tick() {
        if (!BattleTank.Tank.HasComponent<TankStateTimeOutComponent>())
            BattleTank.Tank.AddComponent(new TankStateTimeOutComponent());
    }
}

public class Active(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override DateTimeOffset TimeToNextState { get; } = DateTimeOffset.UtcNow;
    public override IComponent StateComponent { get; } = new TankActiveStateComponent();
}
