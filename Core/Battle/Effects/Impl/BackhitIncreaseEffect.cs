using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;

namespace Vint.Core.Battle.Effects;

public class BackhitIncreaseEffect(
    BattleTank tank,
    int level,
    float multiplier
) : Effect(tank, level), IDamageMultiplierEffect {
    public float Multiplier { get; } = multiplier;

    public float GetMultiplier(BattleTank source, BattleTank target, IWeaponHandler weaponHandler, bool isSplash, bool isBackHit, bool isTurretHit) =>
        (isBackHit || isTurretHit) && source == Tank
            ? Multiplier
            : 1;

    public override Task Activate() =>
        Task.FromResult(Tank.Effects.Add(this));

    public override Task Deactivate() =>
        Task.FromResult(Tank.Effects.TryRemove(this));
}
