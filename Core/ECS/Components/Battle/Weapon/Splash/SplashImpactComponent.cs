using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Splash;

[ProtocolId(1438773081827)]
public class SplashImpactComponent(
    float impactForce
) : IComponent {
    public float ImpactForce { get; set; } = impactForce;
}