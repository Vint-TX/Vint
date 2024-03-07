using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon;

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