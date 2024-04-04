using System.Numerics;

namespace Vint.Core.Config.MapInformation;

public readonly record struct Bonus(
    int Number,
    bool HasParachute,
    Vector3 Position
);