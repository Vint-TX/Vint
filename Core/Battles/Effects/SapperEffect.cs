using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;

namespace Vint.Core.Battles.Effects;

public class SapperEffect(
    float resistance,
    BattleTank tank,
    int level
) : Effect(tank, level), IDamageMultiplierEffect {
    public override Task Activate() => Task.FromResult(Tank.Effects.Add(this));

    public override Task Deactivate() => Task.FromResult(Tank.Effects.TryRemove(this));

    public float Multiplier { get; } = resistance;

    public float GetMultiplier(BattleTank source, BattleTank target, IWeaponHandler weaponHandler, bool isSplash, bool isBackHit, bool isTurretHit) {
        if (target != Tank || weaponHandler is not IMineWeaponHandler)
            return 1;

        Deactivate().GetAwaiter().GetResult();
        return Multiplier;
    }
}
