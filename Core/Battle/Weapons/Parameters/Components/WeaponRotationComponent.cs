using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Parameters.Components;

[ProtocolId(1432792458422)]
public class WeaponRotationComponent(
    float speed,
    float acceleration,
    float baseSpeed
) : IComponent {
    public float Speed { get; set; } = speed;
    public float Acceleration { get; set; } = acceleration;
    public float BaseSpeed { get; set; } = baseSpeed;
}
