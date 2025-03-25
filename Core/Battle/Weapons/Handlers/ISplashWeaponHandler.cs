using Vint.Core.Battle.Weapons.Weapon.Hit;

namespace Vint.Core.Battle.Weapons.Handlers;

public interface ISplashWeaponHandler : IWeaponHandler {
    float MinSplashDamagePercent { get; }
    float RadiusOfMaxSplashDamage { get; }
    float RadiusOfMinSplashDamage { get; }

    Task SplashFire(HitTarget target, int targetIndex);

    float GetSplashMultiplier(float distance);
}
