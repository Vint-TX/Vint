using System.Numerics;

namespace Vint.Core.Battles.Damage;

public readonly record struct CalculatedDamage(
    Vector3 HitPoint,
    float Value,
    bool IsCritical,
    bool IsSpecial
);
