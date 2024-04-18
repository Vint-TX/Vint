using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Components.Battle.Weapon;
using Vint.Core.ECS.Templates.Battle.Effect;

namespace Vint.Core.Battles.Effects;

public class EmergencyProtectionEffect : Effect, IDamageMultiplierEffect {
    public EmergencyProtectionEffect(TimeSpan duration, BattleTank tank, int level) : base(tank, level) => 
        Duration = duration;
    
    public float Multiplier => 1;
    
    public override void Activate() {
        if (IsActive) return;
        
        Tank.Effects.Add(this);
        Tank.Weapon.RemoveComponentIfPresent<ShootableComponent>();
        ResetWeaponState();
        
        Entities.Add(new EmergencyProtectionEffectTemplate().Create(Tank.BattlePlayer, Duration));
        ShareAll();
        
        LastActivationTime = DateTimeOffset.UtcNow;
        Schedule(Duration, Deactivate);
    }
    
    public override void Deactivate() {
        if (!IsActive) return;
        
        Tank.Effects.TryRemove(this);
        Tank.Weapon.AddComponentIfAbsent<ShootableComponent>();
        
        UnshareAll();
        Entities.Clear();
        
        LastActivationTime = default;
    }
    
    public float GetMultiplier(BattleTank source, BattleTank target, bool isSplash) => 0;
    
    void ResetWeaponState() => (Tank.WeaponHandler as StreamWeaponHandler)?.Reset();
}