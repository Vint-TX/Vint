using System.Numerics;
using Vint.Core.Config.MapInformation;

namespace Vint.Core.Utils;

public static class PhysicsUtils {
    extension(Vector3 vector) {
        public bool IsValidVector3() => IsValidFloat(vector.X) && IsValidFloat(vector.Y) && IsValidFloat(vector.Z);
    }

    extension(Quaternion quaternion) {
        public bool IsValidQuaternion() => IsValidFloat(quaternion.W) &&
                                           IsValidFloat(quaternion.X) &&
                                           IsValidFloat(quaternion.Y) &&
                                           IsValidFloat(quaternion.Z);
    }

    extension(float value) {
        public bool IsValidFloat() => !float.IsInfinity(value) && !float.IsNaN(value);
    }

    public static bool CheckOverflow(Vector3 vec) {
        const float maxValue = 655.36f;
        return Math.Abs(vec.X) > maxValue || Math.Abs(vec.Y) > maxValue || Math.Abs(vec.Z) > maxValue;
    }

    public static bool IsInsideBox(Vector3 point, Vector3 center, Vector3 size) => point.X > center.X - size.X / 2 &&
                                                                                   point.Y > center.Y - size.Y / 2 &&
                                                                                   point.Z > center.Z - size.Z / 2 &&
                                                                                   point.X < center.X + size.X / 2 &&
                                                                                   point.Y < center.Y + size.Y / 2 &&
                                                                                   point.Z < center.Z + size.Z / 2;

    public static bool IsOutsideMap(IEnumerable<PuntativeGeometry> puntativeGeometries, Vector3 position, Vector3 velocity, bool killZonesEnabled) =>
        !IsValidVector3(position) ||
        CheckOverflow(position + velocity) ||
        killZonesEnabled && puntativeGeometries.Any(geometry => IsInsideBox(position, geometry.Position, geometry.Size));
}
