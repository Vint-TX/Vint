using System.Numerics;

namespace Vint.Core.Battle.Damage.Calculator;

public readonly record struct CalculatedDamage(
    Vector3 HitPoint,
    float Value,
    bool IsCritical,
    bool IsSpecial
);
