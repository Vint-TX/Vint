using System.Numerics;

namespace Vint.Core.Config.MapInformation;

public readonly record struct SpawnPoint(
    int Number,
    Vector3 Position,
    Quaternion Rotation
) {
    public override string ToString() => $"{Number}. Position: {Position} Rotation: {Rotation}";
}