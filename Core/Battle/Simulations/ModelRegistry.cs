using System.Collections.Concurrent;
using System.Numerics;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Schema2;
using Vint.Core.Battle.Simulations.Geometry;

namespace Vint.Core.Battle.Simulations;

public static class ModelRegistry {
    static ConcurrentDictionary<string, Triangle[]> Cache { get; } = [];
    static string ModelsPath { get; } = Path.Combine("Resources", "Simulation", "Models");

    // todo thread safety (prevent multiple threads from loading the same model at the same time)
    public static Triangle[] GetOrLoad(string path) {
        if (Cache.TryGetValue(path, out Triangle[]? triangles))
            return triangles;

        string realPath = Path.Combine(ModelsPath, path);

        triangles = LoadModel(realPath);
        Cache[path] = triangles;
        return triangles;
    }

    static Triangle[] LoadModel(string path) {
        ModelRoot meshRoot = ModelRoot.Load(path);

        Triangle[] triangles = meshRoot.DefaultScene
            .EvaluateTriangles()
            .Select(tuple => new Triangle(
                CreateVertex(tuple.A.GetGeometry()),
                CreateVertex(tuple.B.GetGeometry()),
                CreateVertex(tuple.C.GetGeometry())))
            .ToArray();

        return triangles;
    }

    static Vertex CreateVertex(IVertexGeometry vertexBuilder) {
        Vector3 position = vertexBuilder.GetPosition();

        if (!vertexBuilder.TryGetNormal(out Vector3 normal))
            normal = Vector3.One;

        return new Vertex(position, normal);
    }
}
