using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Effects;

public class AdrenalineEffect(
    BattleTank tank,
    int level,
    float cooldownSpeedCoeff,
    float damageMultiplier
) : Effect(tank, level), IDamageMultiplierEffect {
    public event Action? Deactivated;
    
    public override void Activate() {
        Tank.Effects.Add(this);
        
        Tank.UpdateModuleCooldownSpeed(cooldownSpeedCoeff);
    }
    
    public override void Deactivate() {
        Tank.Effects.TryRemove(this);
        
        Tank.UpdateModuleCooldownSpeed(1, true);
        Deactivated?.Invoke();
    }
    
    public float Multiplier { get; } = damageMultiplier;
    
    public float GetMultiplier(BattleTank source, BattleTank target, bool isSplash, bool isBackHit, bool isTurretHit) =>
        source == Tank ? Multiplier : 1;
}