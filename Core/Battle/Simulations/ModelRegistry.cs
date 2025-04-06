using System.Collections.Concurrent;
using System.Numerics;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Schema2;
using Vint.Core.Battle.Simulations.Geometry;

namespace Vint.Core.Battle.Simulations;

public static class ModelRegistry {
    static Vector3 GltfToUnity { get; } = new(-1, 1, 1);
    static ConcurrentDictionary<string, Triangle[]> Cache { get; } = [];
    static string ModelsPath { get; } = Path.Combine("Resources", "Simulation", "Models");

    // todo thread safety
    // todo refactor maps and remove isRelative
    public static Triangle[] GetOrLoad(string path, bool isRelative = true) {
        if (Cache.TryGetValue(path, out Triangle[]? triangles))
            return triangles;

        if (isRelative)
            path = Path.Combine(ModelsPath, path);

        triangles = LoadModel(path);
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
