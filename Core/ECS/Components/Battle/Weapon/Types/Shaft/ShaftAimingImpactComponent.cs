using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Shaft;

[ProtocolId(1437983715951)]
public class ShaftAimingImpactComponent(
    float maxImpactForce
) : IComponent {
    public float MaxImpactForce { get; set; } = maxImpactForce;
}