using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons.Handlers;
using Vint.Core.Battle.Weapons.Weapon.Hit;

namespace Vint.Core.Battle.Weapons.Damage.Calculator;

public interface IDamageCalculator {
    Task<CalculatedDamage> Calculate(
        BattleTank source,
        BattleTank target,
        IWeaponHandler weaponHandler,
        HitTarget hitTarget,
        int targetHitIndex,
        bool isSplash = false,
        bool ignoreSourceEffects = false);
}
