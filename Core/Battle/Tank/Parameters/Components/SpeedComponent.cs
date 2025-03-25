using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Parameters.Components;

[ProtocolId(-1745565482362521070)]
public class SpeedComponent(
    float speed,
    float turnSpeed,
    float acceleration
) : IComponent {
    public float Speed { get; set; } = speed;
    public float TurnSpeed { get; set; } = turnSpeed;
    public float Acceleration { get; set; } = acceleration;
}
