using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Components.Battle.Weapon.Splash;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class ThunderWeaponHandler : DiscreteTankWeaponHandler, ISplashWeaponHandler {
    public ThunderWeaponHandler(BattleTank battleTank) : base(battleTank) {
        SplashWeaponComponent splashWeapon = BattleEntity.GetComponent<SplashWeaponComponent>();

        MinSplashDamagePercent = splashWeapon.MinSplashDamagePercent;
        RadiusOfMaxSplashDamage = splashWeapon.RadiusOfMaxSplashDamage;
        RadiusOfMinSplashDamage = splashWeapon.RadiusOfMinSplashDamage;
    }

    public override int MaxHitTargets => 1;

    public float MinSplashDamagePercent { get; }
    public float RadiusOfMaxSplashDamage { get; }
    public float RadiusOfMinSplashDamage { get; }

    public async Task SplashFire(HitTarget target, int targetIndex) {
        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);
        bool isEnemy = targetTank == BattleTank || BattleTank.IsEnemy(targetTank);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy) return;

        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex, true);
        await battle.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }

    public float GetSplashMultiplier(float distance) {
        if (distance <= RadiusOfMaxSplashDamage) return 1;
        if (distance >= RadiusOfMinSplashDamage) return 0;

        return 0.01f *
               (MinSplashDamagePercent +
                (RadiusOfMinSplashDamage - distance) *
                (100f - MinSplashDamagePercent) /
                (RadiusOfMinSplashDamage - RadiusOfMaxSplashDamage));
    }
}
