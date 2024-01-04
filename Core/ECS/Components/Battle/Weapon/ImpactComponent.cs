using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon;

[ProtocolId(1437983636148)]
public class ImpactComponent(
    float impactForce
) : IComponent {
    public float ImpactForce { get; set; } = impactForce;
}