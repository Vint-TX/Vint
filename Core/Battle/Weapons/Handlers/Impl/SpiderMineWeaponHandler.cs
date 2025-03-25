using Vint.Core.Battle.Damage.Calculator;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battle.Weapons;

public class SpiderMineWeaponHandler(
    BattleTank tank,
    IDamageCalculator damageCalculator,
    TimeSpan cooldown,
    IEntity marketEntity,
    IEntity battleEntity,
    bool damageWeakeningByDistance,
    float maxDamageDistance,
    float minDamageDistance,
    float minDamagePercent,
    float maxDamage,
    float minDamage,
    Func<Task> explode
) : ModuleWeaponHandler(tank,
    damageCalculator,
    cooldown,
    marketEntity,
    battleEntity,
    damageWeakeningByDistance,
    maxDamageDistance,
    minDamageDistance,
    minDamagePercent,
    maxDamage,
    minDamage,
    int.MaxValue), IDiscreteWeaponHandler, IMineWeaponHandler {
    public override Task Fire(HitTarget target, int targetIndex) => throw new NotSupportedException();

    public float MinSplashDamagePercent { get; } = minDamagePercent;
    public float RadiusOfMaxSplashDamage { get; } = maxDamageDistance;
    public float RadiusOfMinSplashDamage { get; } = minDamageDistance;

    public async Task Explode() => await explode();

    public async Task SplashFire(HitTarget target, int targetIndex) {
        Round round = BattleTank.Round;
        BattleTank targetTank = round.Tankers
            .Select(tanker => tanker.Tank)
            .Single(tank => tank.Entities.Incarnation == target.IncarnationEntity);

        bool isEnemy = targetTank == BattleTank || BattleTank.IsEnemy(targetTank);

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy)
            return;

        CalculatedDamage damage = await DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex, true, true);
        await round.DamageProcessor.Damage(BattleTank, targetTank, MarketEntity, BattleEntity, damage);
    }

    public float GetSplashMultiplier(float distance) {
        if (distance <= RadiusOfMaxSplashDamage) return 1;
        if (distance >= RadiusOfMinSplashDamage) return 0;

        return 0.01f *
               (MinSplashDamagePercent +
                (RadiusOfMinSplashDamage - distance) * (100f - MinSplashDamagePercent) / (RadiusOfMinSplashDamage - RadiusOfMaxSplashDamage));
    }
}
