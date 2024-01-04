using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon;

[ProtocolId(1437989437781)]
public class KickbackComponent(
    float kickbackForce
) : IComponent {
    public float KickbackForce { get; set; } = kickbackForce;
}