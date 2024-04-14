using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public abstract class ModuleWeaponHandler(
    BattleTank tank,
    TimeSpan cooldown,
    IEntity marketEntity,
    bool damageWeakeningByDistance,
    float maxDamageDistance,
    float minDamageDistance,
    float minDamagePercent,
    float maxDamage,
    float minDamage,
    int maxHitTargets
) : IWeaponHandler {
    public IEntity MarketEntity { get; } = marketEntity;
    public float MinDamage { get; } = minDamage;
    public float MaxDamage { get; } = maxDamage;
    public IDamageCalculator DamageCalculator { get; } = new DamageCalculator();
    public BattleTank BattleTank { get; } = tank;
    public TimeSpan Cooldown { get; } = cooldown;
    public bool DamageWeakeningByDistance { get; } = damageWeakeningByDistance;
    public float MaxDamageDistance { get; } = maxDamageDistance;
    public float MinDamageDistance { get; } = minDamageDistance;
    public float MinDamagePercent { get; } = minDamagePercent;
    public int MaxHitTargets { get; } = maxHitTargets;
    public DateTimeOffset LastHitTime { get; set; }
    
    public abstract void Fire(HitTarget target, int targetIndex);
    
    public virtual void OnTankEnable() { }
    
    public virtual void OnTankDisable() { }
    
    public virtual void Tick() { }
}