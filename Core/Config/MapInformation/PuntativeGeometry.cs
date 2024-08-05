using System.Numerics;

namespace Vint.Core.Config.MapInformation;

public readonly record struct PuntativeGeometry(
    Vector3 Position,
    Vector3 Size
);
