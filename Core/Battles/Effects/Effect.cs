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
    public BattleTank Tank { get; } = tank;
    public List<IEntity> Entities { get; } = new(1);
    public IEntity? Entity => Entities.SingleOrDefaultSafe();
    public Battle Battle => Tank.Battle;

    public int Level { get; protected set; } = level;
    public bool IsSupply => Level < 0;
    public bool IsActive => Entities.Count != 0;
    public bool CanBeDeactivated { get; set; } = true;

    public TimeSpan Duration { get; protected set; } = TimeSpan.FromSeconds(1);

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

    public virtual Task Share(BattlePlayer battlePlayer) => battlePlayer.PlayerConnection.Share(Entities);

    public virtual async Task Unshare(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank == Tank)
            await Deactivate();

        await battlePlayer.PlayerConnection.Unshare(Entities);
    }

    protected async Task ShareAll() {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle))
            await battlePlayer.PlayerConnection.Share(Entities);
    }

    protected async Task UnshareAll() {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle))
            await battlePlayer.PlayerConnection.Unshare(Entities);
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
