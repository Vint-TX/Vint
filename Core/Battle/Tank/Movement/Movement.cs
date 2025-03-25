using System.Numerics;

namespace Vint.Core.Battle.Tank.Movement;

public struct Movement {
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 AngularVelocity { get; set; }
    public Quaternion Orientation { get; set; }
}
