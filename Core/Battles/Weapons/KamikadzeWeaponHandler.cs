using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class KamikadzeWeaponHandler(
    BattleTank tank,
    TimeSpan cooldown,
    IEntity marketEntity,
    IEntity battleEntity,
    bool damageWeakeningByDistance,
    float maxDamageDistance,
    float minDamageDistance,
    float minDamagePercent,
    float maxDamage,
    float minDamage,
    int maxHitTargets
) : ModuleWeaponHandler(
    tank,
    cooldown,
    marketEntity,
    battleEntity,
    damageWeakeningByDistance,
    maxDamageDistance,
    minDamageDistance,
    minDamagePercent,
    maxDamage,
    minDamage,
    maxHitTargets
), ISplashWeaponHandler, IDiscreteWeaponHandler {
    public override Task Fire(HitTarget target, int targetIndex) => throw new NotSupportedException();

    public float MinSplashDamagePercent => MinDamagePercent;
    public float RadiusOfMaxSplashDamage => MaxDamageDistance;
    public float RadiusOfMinSplashDamage => MinDamageDistance;

    public async Task SplashFire(HitTarget target, int targetIndex) {
        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);
        bool isEnemy = BattleTank.IsEnemy(targetTank);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy) return;

        CalculatedDamage damage = DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex, true, true);
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
