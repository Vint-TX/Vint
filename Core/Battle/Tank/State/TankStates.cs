using Vint.Core.Battle.Flags;
using Vint.Core.Battle.Mode.Team.Impl;
using Vint.Core.Battle.Modules.Interfaces;
using Vint.Core.Battle.Modules.Types.Base;
using Vint.Core.Battle.Rounds;
using Vint.Core.Config.MapInformation;
using Vint.Core.ECS.Components;
using Vint.Core.ECS.Components.Battle;
using Vint.Core.ECS.Components.Battle.Movement;
using Vint.Core.ECS.Components.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.StateMachine;

namespace Vint.Core.Battle.Tank;

public class TankStateManager(
    BattleTank tank
) : StateManager<TankState> {
    public BattleTank Tank { get; } = tank;

    public override async Task Init() =>
        await InitState(new New(this));
}

public abstract class TankState(
    TankStateManager stateManager
) : State {
    public override TankStateManager StateManager { get; } = stateManager;
    protected BattleTank Tank => StateManager.Tank;

    public abstract Task ForceRemoveStateComponent();
}

public abstract class TankState<TComponent>(
    TankStateManager stateManager
) : TankState(stateManager) where TComponent : class, IComponent, new() {
    TComponent StateComponent { get; } = new();
    bool ComponentForceRemoved { get; set; }

    public override async Task Start() {
        await Tank.Entities.Tank.AddComponent(StateComponent);
        await base.Start();
    }

    public override async Task Finish() {
        if (!ComponentForceRemoved)
            await Tank.Entities.Tank.RemoveComponent<TComponent>();

        await base.Finish();
    }

    public override async Task ForceRemoveStateComponent() {
        if (ComponentForceRemoved) return;

        ComponentForceRemoved = true;
        await Tank.Entities.Tank.RemoveComponent<TComponent>();
    }
}

public class New(
    TankStateManager stateManager
) : TankState<TankNewStateComponent>(stateManager);

public class Spawn(
    TankStateManager stateManager
) : TankState<TankSpawnStateComponent>(stateManager) {
    DateTimeOffset SemiActiveTime { get; } = DateTimeOffset.UtcNow.AddSeconds(1.75);
    TankEntities Entities => Tank.Entities;
    Round Round => Tank.Round;

    public override async Task Start() {
        SpawnPoint spawnPoint = Round.ModeHandler.GetRandomSpawnPoint(Tank.Tanker);

        await Tank.Disable();

        await Entities.Tank.RemoveComponentIfPresent<TankVisibleStateComponent>();
        await Entities.Tank.RemoveComponentIfPresent<TankMovementComponent>(); // does not present if transitioned from 'New' state
        await Entities.RecreateIncarnation();

        Tank.SetSpawnPoint(spawnPoint);
        await Entities.Tank.AddComponent(new TankMovementComponent(spawnPoint.CreateMovement(), default, 0, 0));
        await base.Start();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        if (DateTimeOffset.UtcNow >= SemiActiveTime)
            await StateManager.SetState(new SemiActive(StateManager));
    }
}

public class SemiActive(
    TankStateManager stateManager
) : TankState<TankSemiActiveStateComponent>(stateManager) {
    bool TimedOut { get; set; }
    DateTimeOffset Timeout { get; } = DateTimeOffset.UtcNow.AddSeconds(1);
    TankEntities Entities => Tank.Entities;
    Round Round => Tank.Round;

    public override async Task Start() {
        await Tank.Enable();
        await Entities.Tank.AddComponent<TankVisibleStateComponent>();
        await base.Start();
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        if (TimedOut) {
            if (await CompareCollisions()) {
                await Entities.Tank.RemoveComponentIfPresent<TankStateTimeOutComponent>();
                await StateManager.SetState(new Active(StateManager));
            }
        } else if (DateTimeOffset.UtcNow >= Timeout) {
            TimedOut = true;
            await Entities.Tank.AddComponent<TankStateTimeOutComponent>();
        }
    }

    async ValueTask<bool> CompareCollisions() {
        IEntity battleModeEntity = Round.ModeHandler.Entity;

        if (Tank.CollisionsPhase != battleModeEntity.GetComponent<BattleTankCollisionsComponent>().SemiActiveCollisionsPhase)
            return false;

        await battleModeEntity.ChangeComponent<BattleTankCollisionsComponent>(component => component.SemiActiveCollisionsPhase++);
        return true;
    }
}

public class Active(
    TankStateManager stateManager
) : TankState<TankActiveStateComponent>(stateManager) {
    public override async Task Start() {
        await base.Start();
        await Tank.SetHealth(Tank.MaxHealth);

        foreach (BattleModule module in Tank.Modules)
            await module.TryUnblock();
    }

    public override async Task Started() {
        await base.Started();

        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (BattleModule module in Tank.Modules.OfType<IAlwaysActiveModule>())
            await module.Activate();
    }

    public override async Task Finish() {
        await base.Finish();

        foreach (BattleModule module in Tank.Modules)
            await module.TryBlock();
    }
}

public class Dead(
    TankStateManager stateManager
) : TankState<TankDeadStateComponent>(stateManager) {
    DateTimeOffset SpawnTime { get; } = DateTimeOffset.UtcNow.AddSeconds(3);

    public override async Task Start() {
        await Tank.Disable();
        await base.Start();

        if (Tank.Round.ModeHandler is not CTFHandler ctf)
            return;

        foreach (Flag flag in ctf.Flags.Values) {
            if (flag.StateManager.CurrentState is not Captured captured || captured.Carrier != Tank.Tanker)
                continue;

            await captured.Drop(false);
        }
    }

    public override async Task Tick(TimeSpan deltaTime) {
        await base.Tick(deltaTime);

        if (!Tank.Tanker.IsPaused && DateTimeOffset.UtcNow >= SpawnTime)
            await StateManager.SetState(new Spawn(StateManager));
    }
}
