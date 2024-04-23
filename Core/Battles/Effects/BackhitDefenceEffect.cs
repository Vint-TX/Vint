using Vint.Core.Battles.Player;

namespace Vint.Core.Battles.Effects;

public class BackhitDefenceEffect(
    BattleTank tank,
    int level,
    float multiplier
) : Effect(tank, level), IDamageMultiplierEffect {
    public override void Activate() =>
        Tank.Effects.Add(this);
    
    public override void Deactivate() =>
        Tank.Effects.TryRemove(this);
    
    public float Multiplier { get; } = multiplier;
    
    public float GetMultiplier(BattleTank source, BattleTank target, bool isSplash, bool isBackHit, bool isTurretHit) =>
        (isBackHit || isTurretHit) && target == Tank ? Multiplier : 1;
}