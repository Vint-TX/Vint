using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.States;

public abstract class TankState(
    TankStateManager stateManager
) : State {
    public abstract IComponent StateComponent { get; }
    public override TankStateManager StateManager { get; } = stateManager;
    public BattleTank BattleTank => StateManager.BattleTank;

    public override void Start() {
        BattleTank.Tank.AddComponent(StateComponent);
        base.Start();
    }

    public override void Finish() {
        BattleTank.Tank.RemoveComponentIfPresent(StateComponent);
        base.Finish();
    }
}

public class New(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankNewStateComponent();
}

public class Dead(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankDeadStateComponent();
    DateTimeOffset TimeToNextState { get; set; }

    public override void Start() { // todo
        BattleTank.Disable();
        base.Start();
        TimeToNextState = DateTimeOffset.UtcNow.AddSeconds(3);
    }

    public override void Tick() {
        if (DateTimeOffset.UtcNow >= TimeToNextState)
            StateManager.SetState(new Spawn(StateManager));
    }
}

public class Spawn(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankSpawnStateComponent();
    DateTimeOffset TimeToNextState { get; set; }

    public override void Start() {
        BattleTank.Disable();
        BattleTank.Spawn();
        base.Start();
        TimeToNextState = DateTimeOffset.UtcNow.AddSeconds(1.75);
    }

    public override void Tick() {
        if (DateTimeOffset.UtcNow >= TimeToNextState)
            StateManager.SetState(new SemiActive(StateManager));
    }
}

public class SemiActive(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankSemiActiveStateComponent();
    DateTimeOffset TimeToNextState { get; set; }

    public override void Start() {
        BattleTank.Enable();
        BattleTank.Tank.ChangeComponent<TemperatureComponent>(component => component.Temperature = 0);
        BattleTank.Tank.AddComponent(new TankVisibleStateComponent());
        base.Start();
        TimeToNextState = DateTimeOffset.UtcNow.AddSeconds(1);
    }

    public override void Tick() {
        if (DateTimeOffset.UtcNow >= TimeToNextState && !BattleTank.Tank.HasComponent<TankStateTimeOutComponent>())
            BattleTank.Tank.AddComponent(new TankStateTimeOutComponent());
    }
}

public class Active(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankActiveStateComponent();
}