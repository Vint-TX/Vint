using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Hammer;

[ProtocolId(1464955716416)]
public class HammerPelletConeComponent : IComponent {
    public float HorizontalConeHalfAngle { get; private set; }
    public float VerticalConeHalfAngle { get; private set; }
    public int PelletCount { get; private set; }
}
