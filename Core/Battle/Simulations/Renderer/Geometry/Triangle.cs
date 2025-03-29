namespace Vint.Core.Battle.Simulations.Renderer.Geometry;

public readonly struct Triangle(
    Vertex a,
    Vertex b,
    Vertex c
) {
    public Vertex A { get; } = a;
    public Vertex B { get; } = b;
    public Vertex C { get; } = c;
}
