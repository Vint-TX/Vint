using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battle.Weapons;

public interface ISplashWeaponHandler : IWeaponHandler {
    float MinSplashDamagePercent { get; }
    float RadiusOfMaxSplashDamage { get; }
    float RadiusOfMinSplashDamage { get; }

    Task SplashFire(HitTarget target, int targetIndex);

    float GetSplashMultiplier(float distance);
}
