using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using Vint.Core.Battle.Lobby.Components;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Simulations.Callbacks;
using Vint.Core.Battle.Simulations.Geometry;
using Vint.Core.Battle.Simulations.Renderer;
using Vint.Core.Structures;
using BepuMesh = BepuPhysics.Collidables.Mesh;
using BepuTriangle = BepuPhysics.Collidables.Triangle;
using Triangle = Vint.Core.Battle.Simulations.Geometry.Triangle;

namespace Vint.Core.Battle.Simulations;

public class RoundSimulation : IDisposable {
    public RoundSimulation(Round round) {
        Round = round;

        float gravity = Round.ModeHandler.Entity.GetComponent<GravityComponent>().Gravity;

        Simulation = Simulation.Create(new BufferPool(), new CollisionCallbacks(), new PoseIntegratorCallbacks(gravity), new SolveDescription(1, 1));
        ThreadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);

#if DEBUG
        Renderer = new RendererWindow($"Simulation {Round.Entity.Id}", Simulation, Dispatcher);
#endif
    }

    static Vector3 GltfToUnity { get; } = new(-1, 1, 1);

    public Round Round { get; }
    public Simulation Simulation { get; }
    public RendererWindow? Renderer { get; }
    public Dispatcher Dispatcher { get; } = new();

    IThreadDispatcher? ThreadDispatcher { get; }

    public StaticHandle AddStaticMesh(Triangle[] triangles, MeshDescription description) => Dispatcher.Invoke(() => {
        Simulation.BufferPool.Take(triangles.Length, out Buffer<BepuTriangle> bepuTriangles);
        ConvertTriangles(triangles, bepuTriangles);

        BepuMesh bepuMesh = new(bepuTriangles, description.Scale, Simulation.BufferPool, ThreadDispatcher);
        TypedIndex meshIndex = Simulation.Shapes.Add(bepuMesh); // todo use single shape for all same meshes
        StaticDescription meshDescription = new(description.Position, description.Orientation, meshIndex);
        StaticHandle meshHandle = Simulation.Statics.Add(meshDescription);

        Renderer?.AddStatic(description.Name, description.ColorName, triangles, meshHandle);
        return meshHandle;
    });

    public BodyHandle AddBodyMesh(Triangle[] triangles, MeshDescription description) => Dispatcher.Invoke(() => {
        Simulation.BufferPool.Take(triangles.Length, out Buffer<BepuTriangle> bepuTriangles);
        ConvertTriangles(triangles, bepuTriangles);

        BepuMesh bepuMesh = new(bepuTriangles, description.Scale, Simulation.BufferPool, ThreadDispatcher);
        TypedIndex meshIndex = Simulation.Shapes.Add(bepuMesh); // todo use single shape for all same meshes

        BodyDescription meshDescription = BodyDescription.CreateDynamic(
            new RigidPose(description.Position, description.Orientation),
            bepuMesh.ComputeClosedInertia(100),
            meshIndex,
            0.01f);

        BodyHandle meshHandle = Simulation.Bodies.Add(meshDescription);

        Renderer?.AddBody(description.Name, description.ColorName, triangles, meshHandle);
        return meshHandle;
    });

    public void RemoveStaticMesh(StaticHandle handle) => Dispatcher.Invoke(() => {
        Simulation.Statics.Remove(handle);
        Renderer?.RemoveStatic(handle);
    });

    public void RemoveBodyMesh(BodyHandle handle) => Dispatcher.Invoke(() => {
        Simulation.Bodies.Remove(handle);
        Renderer?.RemoveBody(handle);
    });

    public async Task Tick(TimeSpan deltaTime) => await Dispatcher.InvokeAsync(async () => {
        Simulation.Timestep((float)deltaTime.TotalSeconds, ThreadDispatcher);

        if (Renderer != null)
            await Renderer.Tick(deltaTime);
    });

    static void ConvertTriangles(in Triangle[] triangles, in Buffer<BepuTriangle> bepuTriangles) {
        for (int i = 0; i < triangles.Length; i++)
            bepuTriangles[i] = ConvertTriangle(triangles[i]);

        return;

        BepuTriangle ConvertTriangle(Triangle triangle) =>
            new(triangle.A.Position * GltfToUnity,
                triangle.B.Position * GltfToUnity,
                triangle.C.Position * GltfToUnity);
    }

    public void Dispose() {
        Simulation.Dispose();
        Renderer?.Dispose();
        Dispatcher.Dispose();

        GC.SuppressFinalize(this);
    }
}
