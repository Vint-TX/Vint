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
        HitTarget hitTarget,
        bool isSplash = false,
        bool ignoreSourceEffects = false);
}

public class DamageCalculator : IDamageCalculator {
    public const float BackHitMultiplier = 1.20f;
    const float TurretHitMultiplier = 2f;

    IRandomSource RandomSource { get; } = new WyRandom();

    public CalculatedDamage Calculate(
        BattleTank source,
        BattleTank target,
        HitTarget hitTarget,
        bool isSplash = false,
        bool ignoreSourceEffects = false) {
        WeaponHandler handler = source.WeaponHandler;
        Vector3 hitPoint = hitTarget.LocalHitPoint;
        float distance = hitTarget.HitDistance;

        float baseDamage = handler switch {
            ShaftWeaponHandler shaftHandler => GetShaftDamage(shaftHandler),
            IsisWeaponHandler isisHandler => GetIsisDamage(isisHandler, source, target),
            HammerWeaponHandler hammerHandler => hammerHandler.DamagePerPellet,
            StreamWeaponHandler streamHandler => GetStreamDamage(streamHandler),
            DiscreteWeaponHandler discreteHandler => GetDiscreteDamage(discreteHandler),
            _ => throw new InvalidOperationException($"Cannot find base damage for {handler.GetType().Name}")
        };

        bool isTurretHit = handler is ShaftWeaponHandler { Aiming: true } && IsTurretHit(hitPoint, target.Tank);
        bool isBackHit = !isTurretHit && IsBackHit(hitPoint, target.Tank);
        bool isCritical = false;

        float weakening = !isSplash && handler.DamageWeakeningByDistance ? GetWeakeningMultiplier(handler, distance) : 1;
        float splash = isSplash && handler is ThunderWeaponHandler thunderHandler ? thunderHandler.GetSplashMultiplier(distance) : 1;
        float effects = GetEffectsMultiplier(source, target, isSplash, ignoreSourceEffects);
        float backHit = isBackHit ? BackHitMultiplier : 1;
        float turretHit = isTurretHit ? TurretHitMultiplier : 1;

        float damage = baseDamage * weakening * splash * effects * backHit * turretHit;

        if (handler is SmokyWeaponHandler smokyHandler)
            isCritical = smokyHandler.TryCalculateCriticalDamage(ref damage);

        return new CalculatedDamage(hitPoint, damage, isCritical, isBackHit, isTurretHit, isSplash);
    }

    float GetShaftDamage(ShaftWeaponHandler shaftHandler) {
        const int magicNumber = 400;
        float baseDamage = shaftHandler.Aiming
                               ? (float)(shaftHandler.AimingDuration.TotalMilliseconds + magicNumber)
                               : GetDiscreteDamage(shaftHandler);

        return MathUtils.Map(baseDamage, 0, 1 / shaftHandler.EnergyDrainPerMs + magicNumber, shaftHandler.MinDamage, shaftHandler.MaxDamage);
    }

    static float GetIsisDamage(IsisWeaponHandler isisHandler, BattleTank sourceTank, BattleTank targetTank) => Convert.ToSingle(
        (sourceTank == targetTank || sourceTank.IsEnemy(targetTank)
             ? isisHandler.DamagePerSecond
             : isisHandler.HealPerSecond) *
        isisHandler.Cooldown.TotalSeconds);

    static float GetStreamDamage(StreamWeaponHandler streamHandler) =>
        Convert.ToSingle(streamHandler.DamagePerSecond * streamHandler.Cooldown.TotalSeconds);

    float GetDiscreteDamage(DiscreteWeaponHandler discreteHandler) {
        float min = discreteHandler.MinDamage;
        float max = discreteHandler.MaxDamage;
        float mean = (min + max) / 2;
        float deviation = (max - min) / 6;
        return ZigguratGaussian.Sample(RandomSource, mean, deviation);
    }

    static float GetWeakeningMultiplier(WeaponHandler handler, float distance) {
        float minDamagePercent = handler.MinDamagePercent;
        float minDamageDistance = handler.MinDamageDistance;
        float maxDamageDistance = handler.MaxDamageDistance;

        if (maxDamageDistance >= minDamageDistance)
            throw new ArgumentException($"{nameof(maxDamageDistance)} must be more than {nameof(minDamageDistance)}");

        return distance < maxDamageDistance
                   ? 1
                   : MathUtils.Map(distance,
                       minDamageDistance,
                       maxDamageDistance,
                       minDamagePercent / 100,
                       1);
    }

    static float GetEffectsMultiplier(BattleTank source, BattleTank target, bool isSplash, bool ignoreSourceEffects) {
        List<IDamageEffect> effects = target.Effects.OfType<IDamageEffect>().ToList();

        if (!ignoreSourceEffects && source != target)
            effects.AddRange(source.Effects.OfType<IDamageEffect>());

        return effects.Aggregate<IDamageEffect, float>(1, (current, damageEffect) => current * damageEffect.GetMultiplier(source, target, isSplash));
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