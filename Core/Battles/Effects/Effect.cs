using System.Runtime.CompilerServices;
using ConcurrentCollections;
using Vint.Core.Battles.Player;
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

    public DateTimeOffset LastActivationTime { get; protected set; }
    public TimeSpan Duration { get; protected set; } = TimeSpan.FromSeconds(1);

    ConcurrentHashSet<DelayedAction> DelayedActions { get; } = [];

    public abstract string ConfigPath { get; }

    public virtual void Tick() {
        foreach (DelayedAction delayedAction in DelayedActions
                     .Where(delayedAction => delayedAction.InvokeAtTime <= DateTimeOffset.UtcNow)) {
            DelayedActions.TryRemove(delayedAction);
            delayedAction.Action();
        }
    }

    public virtual void Activate() => Tank.Effects.Add(this);

    public virtual void Deactivate() => Tank.Effects.TryRemove(this);

    public void Share(BattlePlayer battlePlayer) => battlePlayer.PlayerConnection.Share(Entities);

    public void Unshare(BattlePlayer battlePlayer) {
        if (battlePlayer.Tank == Tank)
            Deactivate();

        battlePlayer.PlayerConnection.Unshare(Entities);
    }

    protected void ShareAll() {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle))
            battlePlayer.PlayerConnection.Share(Entities);
    }

    protected void UnshareAll() {
        foreach (BattlePlayer battlePlayer in Battle.Players.Where(battlePlayer => battlePlayer.InBattle))
            battlePlayer.PlayerConnection.Unshare(Entities);
    }

    protected void Schedule(TimeSpan delay, Action action) =>
        DelayedActions.Add(new DelayedAction(DateTimeOffset.UtcNow + delay, action));

    public void UnScheduleAll() => DelayedActions.Clear();

    public override int GetHashCode() => HashCode.Combine(RuntimeHelpers.GetHashCode(this), GetType().Name, Tank);
}

public interface IMultiplierEffect {
    public float Multiplier { get; }
}

public interface ISupplyEffect {
    public float SupplyMultiplier { get; }
    public float SupplyDurationMs { get; }
}

public interface IExtendableEffect {
    ModuleEffectDurationPropertyComponent DurationsComponent { get; }

    public void Extend(int newLevel);
}

public interface IDamageEffect : IMultiplierEffect {
    public float GetMultiplier(BattleTank source, BattleTank target, bool isSplash);
}

public interface ISpeedEffect : IMultiplierEffect {
    public void UpdateTankSpeed();
}