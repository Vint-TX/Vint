using System.Numerics;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Unit;

[ProtocolId(3441234123559)]
public class DroneMoveConfigComponent : IComponent {
    public Vector3 SpawnPosition { get; set; }
    public Vector3 FlyPosition { get; set; }
    public float RotationSpeed { get; set; }
    public float Acceleration { get; set; }
    public float MoveSpeed { get; set; }
}
