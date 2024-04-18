using Vint.Core.Battles.Flags;
using Vint.Core.Battles.Mode;
using Vint.Core.Battles.Modules.Interfaces;
using Vint.Core.Battles.Modules.Types.Base;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Events.Battle.Damage;
using Vint.Core.ECS.Events.Battle.Weapon;
using Vint.Core.StateMachine;

namespace Vint.Core.Battles.Player;

public abstract class TankState(
    TankStateManager stateManager
) : State {
    public abstract IComponent StateComponent { get; }
    public override TankStateManager StateManager { get; } = stateManager;
    protected BattleTank BattleTank => StateManager.BattleTank;

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
    public override IComponent StateComponent => new TankDeadStateComponent();
    DateTimeOffset TimeToNextState { get; set; }

    public override void Start() {
        BattleTank.Disable(false);

        base.Start();
        TimeToNextState = DateTimeOffset.UtcNow.AddSeconds(3);

        if (BattleTank.Battle.ModeHandler is not CTFHandler ctf) return;

        foreach (Flag flag in ctf.Flags.Values.Where(flag => flag.Carrier == BattleTank.BattlePlayer))
            flag.Drop(false);
    }

    public override void Tick() {
        if (!BattleTank.BattlePlayer.IsPaused && DateTimeOffset.UtcNow >= TimeToNextState)
            StateManager.SetState(new Spawn(StateManager));
    }
}

public class Spawn(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankSpawnStateComponent();
    DateTimeOffset TimeToNextState { get; set; }

    public override void Start() {
        BattleTank.Disable(false);
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
        BattleTank.Tank.AddComponent<TankVisibleStateComponent>();
        base.Start();
        TimeToNextState = DateTimeOffset.UtcNow.AddSeconds(1);
    }

    public override void Tick() {
        if (DateTimeOffset.UtcNow >= TimeToNextState)
            BattleTank.Tank.AddComponentIfAbsent(new TankStateTimeOutComponent());
    }
}

public class Active(
    TankStateManager stateManager
) : TankState(stateManager) {
    public override IComponent StateComponent { get; } = new TankActiveStateComponent();

    public override void Start() {
        base.Start();

        if (BattleTank.WeaponHandler is HammerWeaponHandler hammer)
            BattleTank.BattlePlayer.PlayerConnection.Send(new SetMagazineReadyEvent(), hammer.BattleEntity);
        
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