using System.Numerics;

namespace Vint.Core.Utils;

public static class PhysicsUtils {
    public static bool IsValidVector3(this Vector3 vector) => IsValidFloat(vector.X) && 
                                                              IsValidFloat(vector.Y) && 
                                                              IsValidFloat(vector.Z);

    public static bool IsValidQuaternion(this Quaternion quaternion) => IsValidFloat(quaternion.W) && 
                                                                        IsValidFloat(quaternion.X) && 
                                                                        IsValidFloat(quaternion.Y) && 
                                                                        IsValidFloat(quaternion.Z);

    public static bool IsValidFloat(this float value) => !float.IsInfinity(value) && 
                                                         !float.IsNaN(value);
}