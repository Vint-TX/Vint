using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public interface ISplashWeaponHandler {
    public float MinSplashDamagePercent { get; }
    public float RadiusOfMaxSplashDamage { get; }
    public float RadiusOfMinSplashDamage { get; }

    public Task SplashFire(HitTarget target, int targetIndex);

    public float GetSplashMultiplier(float distance);
}
