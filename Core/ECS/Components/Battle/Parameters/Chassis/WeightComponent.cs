using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Parameters.Chassis;

[ProtocolId(1437571863912)]
public class WeightComponent(
    float weight
) : IComponent {
    public float Weight { get; set; } = weight;
}