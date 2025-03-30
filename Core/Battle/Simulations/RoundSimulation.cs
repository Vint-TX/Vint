using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using OpenTK.Mathematics;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Simulations.Callbacks;
using Vint.Core.Battle.Simulations.Renderer;
using BepuMesh = BepuPhysics.Collidables.Mesh;
using BepuTriangle = BepuPhysics.Collidables.Triangle;
using Mesh = Vint.Core.Battle.Simulations.Renderer.Objects.Mesh;
using Triangle = Vint.Core.Battle.Simulations.Geometry.Triangle;

namespace Vint.Core.Battle.Simulations;

public class RoundSimulation : IDisposable {
    public RoundSimulation(Round round) {
        Round = round;

        Simulation = Simulation.Create(new BufferPool(), new CollisionCallbacks(), new PoseIntegratorCallbacks(), new SolveDescription(1, 1));
        ThreadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);

#if DEBUG
        Renderer = new RendererWindow($"Simulation", Simulation, Statics, Bodies);
#endif
    }

    static Vector3 GltfToUnity { get; } = new(-1, 1, 1);

    public Round Round { get; }
    public Simulation Simulation { get; }
    public RendererWindow? Renderer { get; }

    IThreadDispatcher? ThreadDispatcher { get; }

    Dictionary<StaticHandle, Mesh> Statics { get; } = [];
    Dictionary<BodyHandle, Mesh> Bodies { get; } = [];

    public void AddStaticMesh(Triangle[] triangles, Vector3 scale) {
        Simulation.BufferPool.TakeAtLeast(triangles.Length, out Buffer<BepuTriangle> bepuTriangles);
        ConvertTriangles(triangles, bepuTriangles);

        BepuMesh bepuMesh = new(bepuTriangles, ConvertVector3(scale), Simulation.BufferPool, ThreadDispatcher);
        TypedIndex meshIndex = Simulation.Shapes.Add(bepuMesh);
        StaticDescription meshDescription = new(ConvertVector3(Vector3.Zero), meshIndex);
        StaticHandle meshHandle = Simulation.Statics.Add(meshDescription);

        Mesh mesh = new(triangles, scale);
        Statics.Add(meshHandle, mesh);
    }

    public async Task Tick(TimeSpan deltaTime) {
        Simulation.Timestep((float)deltaTime.TotalSeconds, ThreadDispatcher);

        if (Renderer != null)
            await Renderer.Tick(deltaTime);
    }

    static void ConvertTriangles(in Triangle[] triangles, in Buffer<BepuTriangle> bepuTriangles) {
        for (int i = 0; i < triangles.Length; i++)
            bepuTriangles[i] = ConvertTriangle(triangles[i]);

        return;

        BepuTriangle ConvertTriangle(Triangle triangle) =>
            new(ConvertVector3(triangle.A.Position * GltfToUnity),
                ConvertVector3(triangle.B.Position * GltfToUnity),
                ConvertVector3(triangle.C.Position * GltfToUnity));
    }

    static System.Numerics.Vector3 ConvertVector3(Vector3 vector3) => (System.Numerics.Vector3)vector3;

    public void Dispose() {
        Simulation.Dispose();
        Renderer?.Dispose();

        GC.SuppressFinalize(this);
    }
}
