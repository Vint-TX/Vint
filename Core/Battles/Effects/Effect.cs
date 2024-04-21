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
    
    public virtual void Tick() {
        foreach (DelayedAction delayedAction in DelayedActions
                     .Where(delayedAction => delayedAction.InvokeAtTime <= DateTimeOffset.UtcNow)) {
            DelayedActions.TryRemove(delayedAction);
            delayedAction.Action();
        }
    }
    
    public abstract void Activate();
    
    public abstract void Deactivate();
    
    public virtual void Share(BattlePlayer battlePlayer) => battlePlayer.PlayerConnection.Share(Entities);
    
    public virtual void Unshare(BattlePlayer battlePlayer) {
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

public abstract class DurationEffect : Effect {
    protected DurationEffect(BattleTank tank, int level, string marketConfigPath) : base(tank, level) {
        DurationsComponent = ConfigManager.GetComponent<ModuleEffectDurationPropertyComponent>(marketConfigPath);
        
        if (!IsSupply)
            Duration = TimeSpan.FromMilliseconds(DurationsComponent.UpgradeLevel2Values[Level]);
    }
    
    protected ModuleEffectDurationPropertyComponent DurationsComponent { get; }
}

public interface IMultiplierEffect {
    public float Multiplier { get; }
}

public interface ISupplyEffect {
    public float SupplyMultiplier { get; }
    public float SupplyDurationMs { get; }
}

public interface IExtendableEffect {
    public void Extend(int newLevel);
}

public interface IDamageMultiplierEffect : IMultiplierEffect {
    public float GetMultiplier(BattleTank source, BattleTank target, bool isSplash, bool isBackHit, bool isTurretHit);
}

public interface ISpeedEffect : IMultiplierEffect {
    public void UpdateTankSpeed();
}

public interface IModuleWeaponEffect {
    public ModuleWeaponHandler WeaponHandler { get; }
}