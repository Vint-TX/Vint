using OpenTK.Mathematics;

namespace Vint.Core.Battle.Simulations.Renderer.Geometry;

public readonly struct Vertex(
    Vector3 position,
    Vector3 normal
) {
    public Vector3 Position { get; } = position;
    public Vector3 Normal { get; } = normal;
}
