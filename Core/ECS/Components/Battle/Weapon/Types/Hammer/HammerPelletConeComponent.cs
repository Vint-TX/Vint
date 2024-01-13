using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Hammer;

[ProtocolId(1464955716416)]
public class HammerPelletConeComponent(
    float horizontalConeHalfAngle,
    float verticalConeHalfAngle,
    int pelletCount
) : IComponent {
    public float HorizontalConeHalfAngle { get; set; } = horizontalConeHalfAngle;
    public float VerticalConeHalfAngle { get; set; } = verticalConeHalfAngle;
    public int PelletCount { get; set; } = pelletCount;
}