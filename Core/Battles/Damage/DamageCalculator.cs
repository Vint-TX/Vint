using System.Numerics;
using Redzen.Numerics.Distributions.Float;
using Redzen.Random;
using Vint.Core.Battles.Effects;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Weapons;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Battle.Weapon.Hit;
using MathUtils = Vint.Core.Utils.MathUtils;

namespace Vint.Core.Battles.Damage;

public interface IDamageCalculator {
    public CalculatedDamage Calculate(
        BattleTank source,
        BattleTank target,
        IWeaponHandler weaponHandler,
        HitTarget hitTarget,
        int targetHitIndex,
        bool isSplash = false,
        bool ignoreSourceEffects = false);
}

public class DamageCalculator : IDamageCalculator {
    const float BackHitMultiplier = 1.2f;
    const float TurretHitMultiplier = 2f;

    IRandomSource RandomSource { get; } = new WyRandom();

    public CalculatedDamage Calculate(
        BattleTank source,
        BattleTank target,
        IWeaponHandler weaponHandler,
        HitTarget hitTarget,
        int targetHitIndex,
        bool isSplash = false,
        bool ignoreSourceEffects = false) {
        Vector3 hitPoint = hitTarget.LocalHitPoint;
        float distance = hitTarget.HitDistance;
        bool isEnemy = source.IsEnemy(target);
        bool smokyProgressionIsBig = false;

        float baseDamage = weaponHandler switch {
            ShaftWeaponHandler shaftHandler => GetShaftDamage(shaftHandler),
            IsisWeaponHandler isisHandler => GetIsisDamage(isisHandler, source, target, isEnemy),
            RailgunWeaponHandler railgunHandler => GetRailgunDamage(railgunHandler, GetDiscreteDamage(railgunHandler), targetHitIndex),
            HammerWeaponHandler hammerHandler => hammerHandler.DamagePerPellet,
            SmokyWeaponHandler smokyHandler => smokyHandler.GetProgressedDamage(target.Incarnation.Id, out smokyProgressionIsBig),
            StreamWeaponHandler streamHandler => GetStreamDamage(streamHandler),
            IDiscreteWeaponHandler discreteHandler => GetDiscreteDamage(discreteHandler),
            _ => throw new InvalidOperationException($"Cannot find base damage for {weaponHandler.GetType().Name}")
        };

        bool isTurretHit = weaponHandler is ShaftWeaponHandler { Aiming: true } && IsTurretHit(hitPoint, target.Tank);
        bool isBackHit = !isTurretHit && (weaponHandler is not IsisWeaponHandler || isEnemy) && IsBackHit(hitPoint, target.Tank);

        float weakening = !isSplash && weaponHandler.DamageWeakeningByDistance ? GetWeakeningMultiplier(weaponHandler, distance) : 1;
        float splash = isSplash && weaponHandler is ISplashWeaponHandler splashHandler ? splashHandler.GetSplashMultiplier(distance) : 1;
        float effects = GetEffectsMultiplier(source, target, isSplash, isBackHit, isTurretHit, ignoreSourceEffects);
        float backHit = isBackHit ? BackHitMultiplier : 1;
        float turretHit = isTurretHit ? TurretHitMultiplier : 1;

        float damage = baseDamage * weakening * splash * effects * backHit * turretHit;
        return new CalculatedDamage(hitPoint, damage, false, isBackHit || isTurretHit || smokyProgressionIsBig);
    }

    static float GetShaftDamage(ShaftWeaponHandler shaftHandler) =>
        shaftHandler.Aiming
            ? (float)MathUtils.Map(shaftHandler.AimingDuration.TotalMilliseconds,
                0,
                1 / shaftHandler.EnergyDrainPerMs,
                shaftHandler.MinDamage,
                shaftHandler.MaxDamage)
            : shaftHandler.MinDamage;

    static float GetIsisDamage(IsisWeaponHandler isisHandler, BattleTank sourceTank, BattleTank targetTank, bool isEnemy) => Convert.ToSingle(
        (sourceTank == targetTank || isEnemy
             ? isisHandler.DamagePerSecond
             : isisHandler.HealPerSecond) *
        isisHandler.Cooldown.TotalSeconds);

    static float GetRailgunDamage(RailgunWeaponHandler railgunHandler, float baseDamage, int targetHitIndex) =>
        baseDamage * MathF.Pow(railgunHandler.DamageWeakeningByTargetPercent, targetHitIndex);

    static float GetStreamDamage(StreamWeaponHandler streamHandler) =>
        Convert.ToSingle(streamHandler.DamagePerSecond * streamHandler.Cooldown.TotalSeconds);

    float GetDiscreteDamage(IDiscreteWeaponHandler discreteHandler) {
        float min = discreteHandler.MinDamage;
        float max = discreteHandler.MaxDamage;
        float mean = (max + min) / 2;
        float deviation = (max - min) / 6;
        return ZigguratGaussian.Sample(RandomSource, mean, deviation);
    }

    static float GetWeakeningMultiplier(IWeaponHandler handler, float distance) {
        if (!handler.DamageWeakeningByDistance) return 1;

        float minDamagePercent = handler.MinDamagePercent;
        float minDamageDistance = handler.MinDamageDistance;
        float maxDamageDistance = handler.MaxDamageDistance;
        float minMultiplier = minDamagePercent / 100;

        if (maxDamageDistance > minDamageDistance)
            throw new ArgumentException($"{nameof(minDamageDistance)} must be more than {nameof(maxDamageDistance)}");

        return distance <= maxDamageDistance ? 1
               : distance >= minDamageDistance ? minMultiplier
               : MathUtils.Map(distance, minDamageDistance, maxDamageDistance, minMultiplier, 1);
    }

    static float GetEffectsMultiplier(
        BattleTank source,
        BattleTank target,
        bool isSplash,
        bool isBackHit,
        bool isTurretHit,
        bool ignoreSourceEffects) {
        List<IDamageMultiplierEffect> effects = target.Effects.OfType<IDamageMultiplierEffect>().ToList();

        if (!ignoreSourceEffects && source != target)
            effects.AddRange(source.Effects.OfType<IDamageMultiplierEffect>());

        return effects.Aggregate<IDamageMultiplierEffect, float>(1,
            (current, damageEffect) => current * damageEffect.GetMultiplier(source, target, isSplash, isBackHit, isTurretHit));
    }

    static bool IsBackHit(Vector3 hitPoint, IEntity hull) => // "magic" numbers
        hitPoint.Z <
        hull.TemplateAccessor?.ConfigPath?.Split('/').Last() switch {
            "dictator" => -1.9,
            "mammoth" => -1.8,
            "viking" => -1.6,
            _ => -1.25
        };

    static bool IsTurretHit(Vector3 hitPoint, IEntity hull) => // "magic" numbers
        hitPoint.Y >
        hull.TemplateAccessor?.ConfigPath?.Split('/').Last() switch {
            "dictator" => 2.015,
            "hornet" => 1.37,
            "mammoth" => 1.74,
            "titan" => 1.6265,
            "viking" => 1.2955,
            _ => 1.51
        };
}
