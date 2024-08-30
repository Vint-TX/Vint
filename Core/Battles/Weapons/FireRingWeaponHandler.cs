using Vint.Core.Battles.Tank;
using Vint.Core.Battles.Tank.Temperature;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;

namespace Vint.Core.Battles.Weapons;

public class FireRingWeaponHandler(
    BattleTank tank,
    TimeSpan cooldown,
    IEntity marketEntity,
    IEntity battleEntity,
    float temperatureLimit,
    float temperatureDelta,
    float maxHeatDamage,
    float minDamagePercent,
    float minDamageRadius,
    float maxDamageRadius
) : ModuleWeaponHandler(tank, cooldown, marketEntity, battleEntity, false, 0, 0, 0, 0, 0, int.MaxValue), IHeatWeaponHandler, ISplashWeaponHandler {
    public TimeSpan TemperatureDuration => TimeSpan.Zero;
    public float TemperatureLimit { get; } = temperatureLimit;
    public float TemperatureDelta { get; } = temperatureDelta;
    public float HeatDamage { get; private set; }
    public float MinSplashDamagePercent { get; } = minDamagePercent;
    public float RadiusOfMinSplashDamage { get; } = minDamageRadius;
    public float RadiusOfMaxSplashDamage { get; } = maxDamageRadius;

    public override Task Fire(HitTarget target, int targetIndex) => throw new NotSupportedException();

    public Task SplashFire(HitTarget target, int targetIndex) {
        Battle battle = BattleTank.Battle;
        BattleTank targetTank = battle.Players
            .Where(battlePlayer => battlePlayer.InBattleAsTank)
            .Select(battlePlayer => battlePlayer.Tank!)
            .Single(battleTank => battleTank.Incarnation == target.IncarnationEntity);

        float multiplier = GetSplashMultiplier(target.HitDistance);

        if (multiplier <= 0)
            return Task.CompletedTask;

        HeatDamage = maxHeatDamage * multiplier;

        bool isEnemy = BattleTank.IsEnemy(targetTank);

        TemperatureAssist assist = TemperatureCalculator.Calculate(BattleTank, this, !isEnemy);
        targetTank.TemperatureProcessor.EnqueueAssist(assist);

        return Task.CompletedTask;
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
