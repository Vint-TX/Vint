using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;

namespace Vint.Core.Physics;

public struct RayHitHandler : IRayHitHandler {
    public Vector3? ClosestHit { get; private set; }

    public bool AllowTest(CollidableReference collidable) => true;

    public bool AllowTest(CollidableReference collidable, int childIndex) => true;

    public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex) {
        Vector3 hit = ray.Origin + ray.Direction * t;

        if (!ClosestHit.HasValue) {
            ClosestHit = hit;
            return;
        }

        float distanceFromLastHit = Vector3.Distance(ray.Origin, ClosestHit.Value);

        if (t < distanceFromLastHit)
            ClosestHit = hit;
    }
}