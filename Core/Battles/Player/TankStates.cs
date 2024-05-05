using Vint.Core.Battles.Flags;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.Player;

public abstract class TankState(
    TankStateManager stateManager
) : State {
    public abstract IComponent StateComponent { get; }
    public override TankStateManager StateManager { get; } = stateManager;
    protected BattleTank BattleTank => StateManager.BattleTank;

    public override async Task Start() {
        BattleTank.Tank.AddComponent(StateComponent);
        await base.Start();
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
    public override IComponent StateComponent => new TankDeadStateComponent();
    DateTimeOffset TimeToNextState { get; set; }

    public override async Task Start() {
        BattleTank.Disable(false);

        await base.Start();
        TimeToNextState = DateTimeOffset.UtcNow.AddSeconds(3);

        if (BattleTank.Battle.ModeHandler is not CTFHandler ctf) return;

        foreach (Flag flag in ctf.Flags.Where(flag => flag.Carrier == BattleTank.BattlePlayer))
            await flag.Drop(false);
    }

    public override async Task Tick() {
        if (!BattleTank.BattlePlayer.IsPaused && DateTimeOffset.UtcNow >= TimeToNextState)
            StateManager.SetState(new Spawn(StateManager));

        await base.Tick();
    }
}

public class Spawn(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankSpawnStateComponent();
    DateTimeOffset TimeToNextState { get; set; }

    public override async Task Start() {
        BattleTank.Disable(false);
        BattleTank.Spawn();
        await base.Start();
        TimeToNextState = DateTimeOffset.UtcNow.AddSeconds(1.75);
    }

    public override async Task Tick() {
        if (DateTimeOffset.UtcNow >= TimeToNextState)
            StateManager.SetState(new SemiActive(StateManager));

        await base.Tick();
    }
}

public class SemiActive(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankSemiActiveStateComponent();
    DateTimeOffset TimeToNextState { get; set; }

    public override async Task Start() {
        BattleTank.Enable();
        BattleTank.Tank.AddComponent<TankVisibleStateComponent>();
        await base.Start();
        TimeToNextState = DateTimeOffset.UtcNow.AddSeconds(1);
    }

    public override async Task Tick() {
        if (DateTimeOffset.UtcNow >= TimeToNextState)
            BattleTank.Tank.AddComponentIfAbsent(new TankStateTimeOutComponent());

        await base.Tick();
    }
}

public class Active(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankActiveStateComponent();

    public override async Task Start() {
        await base.Start();

        foreach (BattleModule module in BattleTank.Modules)
            module.TryUnblock();
    }

    public override void Started() {
        base.Started();

        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (BattleModule module in BattleTank.Modules.OfType<IAlwaysActiveModule>())
            module.Activate();
    }

    public override void Finish() {
        base.Finish();

        foreach (BattleModule module in BattleTank.Modules)
            module.TryBlock(true);
    }
}
