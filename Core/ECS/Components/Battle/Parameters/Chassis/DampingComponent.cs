using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Parameters.Chassis;

[ProtocolId(1437725485852)]
public class DampingComponent(
    float damping
) : IComponent {
    public float Damping { get; set; } = damping;
}