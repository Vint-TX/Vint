using System.Numerics;

namespace Vint.Core.Config.MapInformation;

public readonly record struct TeleportPoint(
    string Name,
    Vector3 Position,
    Quaternion Rotation
);