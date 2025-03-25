using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Parameters.Components;

[ProtocolId(1437983636148)]
public class ImpactComponent(
    float impactForce
) : IComponent {
    public float ImpactForce { get; set; } = impactForce;
}
