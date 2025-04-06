using System.Numerics;

namespace Vint.Core.Battle.Simulations.Geometry;

public readonly struct MeshDescription() {
    public string Name { get; init; } = "Mesh";
    public string? ColorName { get; init; } = null;
    public Vector3 Position { get; init; } = Vector3.Zero;
    public Quaternion Orientation { get; init; } = Quaternion.Identity;
    public Vector3 Scale { get; init; } = Vector3.One;
}
