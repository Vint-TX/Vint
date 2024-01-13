using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(-7212768015824297898)]
public class ShaftAimingSpeedComponent(
    float horizontalAcceleration,
    float maxHorizontalSpeed,
    float maxVerticalSpeed,
    float verticalAcceleration
) : IComponent {
    public float HorizontalAcceleration { get; set; } = horizontalAcceleration;
    public float MaxHorizontalSpeed { get; set; } = maxHorizontalSpeed;
    public float MaxVerticalSpeed { get; set; } = maxVerticalSpeed;
    public float VerticalAcceleration { get; set; } = verticalAcceleration;
}