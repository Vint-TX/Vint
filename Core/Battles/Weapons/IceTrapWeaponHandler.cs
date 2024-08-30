using Vint.Core.Battles.Damage;
using Vint.Core.Battles.Tank;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class IceTrapWeaponHandler(
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
    float temperatureDelta,
    float temperatureLimit,
    TimeSpan temperatureDuration,
    Func<Task> explode
) : ModuleWeaponHandler(tank,
    cooldown,
    marketEntity,
    battleEntity,
    damageWeakeningByDistance,
    maxDamageDistance,
    minDamageDistance,
    minDamagePercent,
    maxDamage,
    minDamage,
    int.MaxValue), IDiscreteWeaponHandler, IMineWeaponHandler, ITemperatureWeaponHandler {
    public float MinSplashDamagePercent { get; } = minDamagePercent;
    public float RadiusOfMaxSplashDamage { get; } = maxDamageDistance;
    public float RadiusOfMinSplashDamage { get; } = minDamageDistance;

    public float TemperatureLimit { get; } = temperatureLimit;
    public float TemperatureDelta { get; } = temperatureDelta;
    public TimeSpan TemperatureDuration { get; } = temperatureDuration;

    public async Task Explode() => await explode();

    public override Task Fire(HitTarget target, int targetIndex) => throw new NotSupportedException();

    public async Task SplashFire(HitTarget target, int targetIndex) {
        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);
        bool isEnemy = targetTank == BattleTank || BattleTank.IsEnemy(targetTank);

        // todo freeze the target

        if (targetTank.StateManager.CurrentState is not Active || !isEnemy) return;

        CalculatedDamage damage = await DamageCalculator.Calculate(BattleTank, targetTank, this, target, targetIndex, true, true);
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
