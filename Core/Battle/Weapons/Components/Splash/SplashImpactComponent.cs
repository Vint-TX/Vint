using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Splash;

[ProtocolId(1438773081827)]
public class SplashImpactComponent(
    float impactForce
) : IComponent {
    public float ImpactForce { get; set; } = impactForce;
}
