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
        await BattleTank.Tank.AddComponent(StateComponent);
        await base.Start();
    }

    public override async Task Finish() {
        await BattleTank.Tank.RemoveComponentIfPresent(StateComponent);
        await base.Finish();
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
        await BattleTank.Disable(false);

        await base.Start();
        TimeToNextState = DateTimeOffset.UtcNow.AddSeconds(3);

        if (BattleTank.Battle.ModeHandler is not CTFHandler ctf) return;

        foreach (Flag flag in ctf.Flags.Where(flag => flag.Carrier == BattleTank.BattlePlayer))
            await flag.Drop(false);
    }

    public override async Task Tick() {
        if (!BattleTank.BattlePlayer.IsPaused && DateTimeOffset.UtcNow >= TimeToNextState)
            await StateManager.SetState(new Spawn(StateManager));

        await base.Tick();
    }
}

public class Spawn(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankSpawnStateComponent();
    DateTimeOffset TimeToNextState { get; set; }

    public override async Task Start() {
        await BattleTank.Disable(false);
        await BattleTank.Spawn();
        await base.Start();
        TimeToNextState = DateTimeOffset.UtcNow.AddSeconds(1.75);
    }

    public override async Task Tick() {
        if (DateTimeOffset.UtcNow >= TimeToNextState)
            await StateManager.SetState(new SemiActive(StateManager));

        await base.Tick();
    }
}

public class SemiActive(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankSemiActiveStateComponent();
    DateTimeOffset TimeToNextState { get; set; }

    public override async Task Start() {
        await BattleTank.Enable();
        await BattleTank.Tank.AddComponent<TankVisibleStateComponent>();
        await base.Start();
        TimeToNextState = DateTimeOffset.UtcNow.AddSeconds(1);
    }

    public override async Task Tick() {
        if (DateTimeOffset.UtcNow >= TimeToNextState)
            await BattleTank.Tank.AddComponentIfAbsent(new TankStateTimeOutComponent());

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
            await module.TryUnblock();
    }

    public override async Task Started() {
        await base.Started();

        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (BattleModule module in BattleTank.Modules.OfType<IAlwaysActiveModule>())
            await module.Activate();
    }

    public override async Task Finish() {
        await base.Finish();

        foreach (BattleModule module in BattleTank.Modules)
            await module.TryBlock(true);
    }
}
