using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Shaft;

[ProtocolId(1437983715951)]
public class ShaftAimingImpactComponent(
    float maxImpactForce
) : IComponent {
    public float MaxImpactForce { get; set; } = maxImpactForce;
}
