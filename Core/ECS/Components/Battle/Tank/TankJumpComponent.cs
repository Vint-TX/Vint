using System.Numerics;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Tank;

[ProtocolId(1835748384321), ClientAddable, ClientChangeable, ClientRemovable]
public class TankJumpComponent : IComponent {
    public float StartTime { get; private set; }
    public Vector3 Velocity { get; private set; }
    public bool OnFly { get; private set; }
    public bool Slowdown { get; private set; }
    public float SlowdownStartTime { get; private set; }
}