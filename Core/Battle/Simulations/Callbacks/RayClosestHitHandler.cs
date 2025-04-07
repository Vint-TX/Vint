using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;

namespace Vint.Core.Battle.Simulations.Callbacks;

public struct RayClosestHitHandler : IRayHitHandler {
    public Vector3? ClosestHit { get; private set; }

    public bool AllowTest(CollidableReference collidable) => true;

    public bool AllowTest(CollidableReference collidable, int childIndex) => true;

    public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex) {
        maximumT = t;
        ClosestHit = ray.Origin + ray.Direction * t;
    }
}
