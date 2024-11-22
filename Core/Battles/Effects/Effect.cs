using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using ConcurrentCollections;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Weapons;
using Vint.Core.Config;
using Vint.Core.ECS.Components.Server.Modules.Effect.Common;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Effect;
using Vint.Core.Server.Game;
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
    [MemberNotNullWhen(true, nameof(Entity))]
    public bool IsActive => Entity != null;
    public bool CanBeDeactivated { get; set; } = true;

    protected TimeSpan Duration { get; set; } = TimeSpan.Zero;

    ConcurrentHashSet<DelayedAction> DelayedActions { get; } = [];
    ConcurrentHashSet<DelayedTask> DelayedTasks { get; } = [];

    public virtual async Task Tick(TimeSpan deltaTime) {
        foreach (DelayedAction delayedAction in DelayedActions.Where(delayedAction => delayedAction.InvokeAtTime <= DateTimeOffset.UtcNow)) {
            DelayedActions.TryRemove(delayedAction);
            delayedAction.Action();
        }

        foreach (DelayedTask delayedTask in DelayedTasks.Where(delayedTask => delayedTask.InvokeAtTime <= DateTimeOffset.UtcNow)) {
            DelayedTasks.TryRemove(delayedTask);
            await delayedTask.Task();
        }
    }

    public abstract Task Activate();

    public abstract Task Deactivate();

    public virtual async Task DeactivateByEMP() =>
        await Deactivate();

    public virtual async Task Share(BattlePlayer battlePlayer) {
        if (Entity != null)
            await battlePlayer.PlayerConnection.Share(Entity);
    }

    public virtual async Task Unshare(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank == Tank) {
            CanBeDeactivated = true;
            await Deactivate();
            return;
        }

        if (Entity != null) {
            await battlePlayer.PlayerConnection.Send(new RemoveEffectEvent(), Entity);
            await battlePlayer.PlayerConnection.Unshare(Entity);
        }
    }

    protected virtual async Task ShareToAllPlayers() {
        if (Entity != null) {
            foreach (IPlayerConnection connection in Battle
                         .Players
                         .Where(player => player.InBattle)
                         .Select(player => player.PlayerConnection))
                await connection.Share(Entity);
        }
    }

    protected virtual async Task UnshareFromAllPlayers() {
        if (Entity != null) {
            foreach (IPlayerConnection connection in Battle
                         .Players
                         .Where(player => player.InBattle)
                         .Select(player => player.PlayerConnection)) {
                await connection.Send(new RemoveEffectEvent(), Entity);
                await connection.Unshare(Entity);
            }
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

    public override int GetHashCode() => HashCode.Combine(RuntimeHelpers.GetHashCode(this),
        GetType()
            .Name,
        Tank);
}

public abstract class DurationEffect : Effect {
    protected DurationEffect(BattleTank tank, int level, string marketConfigPath) : base(tank, level) {
        if (!IsSupply)
            Duration = TimeSpan.FromMilliseconds(Leveling.GetStat<ModuleEffectDurationPropertyComponent>(marketConfigPath, Level));
    }
}

public abstract class WeaponEffect(
    BattleTank tank,
    int level
) : Effect(tank, level) {
    public abstract ModuleWeaponHandler WeaponHandler { get; protected set; }
    public IEntity WeaponEntity { get; protected set; } = null!;

    public override async Task Share(BattlePlayer battlePlayer) {
        HashSet<IEntity> entities = [WeaponEntity];

        if (Entity != null)
            entities.Add(Entity);

        await battlePlayer.PlayerConnection.Share(entities);

        if (Entity != null)
            await battlePlayer.PlayerConnection.Send(new EffectActivationEvent(), Entity);
    }

    public override async Task Unshare(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank == Tank) {
            CanBeDeactivated = true;
            await Deactivate();
            return;
        }

        HashSet<IEntity> entities = [WeaponEntity];

        if (Entity != null) {
            await battlePlayer.PlayerConnection.Send(new RemoveEffectEvent(), Entity);
            entities.Add(Entity);
        }

        await battlePlayer.PlayerConnection.Unshare(entities);
    }

    protected override async Task ShareToAllPlayers() {
        HashSet<IEntity> entities = [WeaponEntity];

        if (Entity != null)
            entities.Add(Entity);

        foreach (IPlayerConnection connection in Battle
                     .Players
                     .Where(player => player.InBattle)
                     .Select(player => player.PlayerConnection)) {
            await connection.Share(entities);

            if (Entity != null)
                await connection.Send(new EffectActivationEvent(), Entity);
        }
    }

    protected override async Task UnshareFromAllPlayers() {
        HashSet<IEntity> entities = [WeaponEntity];

        if (Entity != null)
            entities.Add(Entity);

        foreach (IPlayerConnection connection in Battle
                     .Players
                     .Where(player => player.InBattle)
                     .Select(player => player.PlayerConnection)) {
            if (Entity != null)
                await connection.Send(new RemoveEffectEvent(), Entity);

            await connection.Unshare(entities);
        }
    }
}

public interface IMultiplierEffect {
    float Multiplier { get; }
}

public interface ISupplyEffect {
    float SupplyMultiplier { get; }
    float SupplyDurationMs { get; }
}

public interface IExtendableEffect {
    Task Extend(int newLevel);
}

public interface IDamageMultiplierEffect : IMultiplierEffect {
    float GetMultiplier(BattleTank source, BattleTank target, IWeaponHandler weaponHandler, bool isSplash, bool isBackHit, bool isTurretHit);
}

public interface IMineEffect {
    BattleTank Owner { get; }
    int Index { get; }
    Vector3 Position { get; }
    float TriggeringArea { get; }

    void TryExplode();
}
