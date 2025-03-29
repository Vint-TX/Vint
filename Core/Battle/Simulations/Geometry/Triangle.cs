namespace Vint.Core.Battle.Simulations.Geometry;

public readonly struct Triangle(
    Vertex a,
    Vertex b,
    Vertex c
) {
    public readonly Vertex A = a;
    public readonly Vertex B = b;
    public readonly Vertex C = c;
}
