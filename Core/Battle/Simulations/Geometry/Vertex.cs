using System.Numerics;

namespace Vint.Core.Battle.Simulations.Geometry;

public readonly struct Vertex(
    Vector3 position,
    Vector3 normal
) {
    public readonly Vector3 Position = position;
    public readonly Vector3 Normal = normal;
}
