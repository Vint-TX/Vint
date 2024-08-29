using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Weapons;

namespace Vint.Core.Battles.Effects;

public class BackhitIncreaseEffect(
    BattleTank tank,
    int level,
    float multiplier
) : Effect(tank, level), IDamageMultiplierEffect {
    public override Task Activate() =>
        Task.FromResult(Tank.Effects.Add(this));

    public override Task Deactivate() =>
        Task.FromResult(Tank.Effects.TryRemove(this));

    public float Multiplier { get; } = multiplier;

    public float GetMultiplier(BattleTank source, BattleTank target, IWeaponHandler weaponHandler, bool isSplash, bool isBackHit, bool isTurretHit) =>
        (isBackHit || isTurretHit) && source == Tank ? Multiplier : 1;
}
