using System.Runtime.CompilerServices;
using ConcurrentCollections;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server.Effect;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Battles.Effects;

public abstract class Effect(
    BattleTank tank,
    int level
) {
    protected BattleTank Tank { get; } = tank;
    protected Battle Battle => Tank.Battle;
    public IEntity? Entity { get; protected set; }

    protected int Level { get; set; } = level;
    protected bool IsSupply => Level < 0;
    protected bool IsActive => Entity != null;
    public bool CanBeDeactivated { get; set; } = true;

    protected TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);

    ConcurrentHashSet<DelayedAction> DelayedActions { get; } = [];
    ConcurrentHashSet<DelayedTask> DelayedTasks { get; } = [];

    public virtual async Task Tick() {
        foreach (DelayedAction delayedAction in DelayedActions
                     .Where(delayedAction => delayedAction.InvokeAtTime <= DateTimeOffset.UtcNow)) {
            DelayedActions.TryRemove(delayedAction);
            delayedAction.Action();
        }

        foreach (DelayedTask delayedTask in DelayedTasks
                     .Where(delayedTask => delayedTask.InvokeAtTime <= DateTimeOffset.UtcNow)) {
            DelayedTasks.TryRemove(delayedTask);
            await delayedTask.Task();
        }
    }

    public abstract Task Activate();

    public abstract Task Deactivate();

    public virtual async Task Share(BattlePlayer battlePlayer) {
        if (Entity != null)
            await battlePlayer.PlayerConnection.Share(Entity);
    }

    public virtual async Task Unshare(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank == Tank) {
            await Deactivate();
            return;
        }

        if (Entity != null)
            await battlePlayer.PlayerConnection.Unshare(Entity);
    }

    protected async Task ShareAll() {
        if (Entity != null) {
            foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle))
                await battlePlayer.PlayerConnection.Share(Entity);
        }
    }

    protected async Task UnshareAll() {
        if (Entity != null) {
            foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle))
                await battlePlayer.PlayerConnection.Unshare(Entity);
        }
    }

    protected void Schedule(TimeSpan delay, Action action) =>
        DelayedActions.Add(new DelayedAction(DateTimeOffset.UtcNow + delay, action));

    protected void Schedule(TimeSpan delay, Func<Task> task) =>
        DelayedTasks.Add(new DelayedTask(DateTimeOffset.UtcNow + delay, task));

    public void UnScheduleAll() {
        DelayedActions.Clear();
        DelayedTasks.Clear();
    }

    public override int GetHashCode() => HashCode.Combine(RuntimeHelpers.GetHashCode(this), GetType().Name, Tank);
}

public abstract class DurationEffect : Effect {
    protected DurationEffect(BattleTank tank, int level, string marketConfigPath) : base(tank, level) {
        if (!IsSupply)
            Duration = TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleEffectDurationPropertyComponent>(marketConfigPath, Level));
    }
}

public interface IMultiplierEffect {
    public float Multiplier { get; }
}

public interface ISupplyEffect {
    public float SupplyMultiplier { get; }
    public float SupplyDurationMs { get; }
}

public interface IExtendableEffect {
    public Task Extend(int newLevel);
}

public interface IDamageMultiplierEffect : IMultiplierEffect {
    public float GetMultiplier(BattleTank source, BattleTank target, bool isSplash, bool isBackHit, bool isTurretHit);
}

public interface IModuleWeaponEffect {
    public ModuleWeaponHandler WeaponHandler { get; }
}
