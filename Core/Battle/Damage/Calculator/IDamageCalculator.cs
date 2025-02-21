using Vint.Core.Battle.Tank;
using Vint.Core.Battle.Weapons;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battle.Damage.Calculator;

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
