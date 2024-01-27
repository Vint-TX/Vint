using System.Numerics;

namespace Vint.Core.Battles.Weapons.Damage;

public readonly record struct CalculatedDamage(
    Vector3 HitPoint,
    float Value,
    bool IsCritical,
    bool IsBackHit,
    bool IsTurretHit,
    bool IsSplash
);