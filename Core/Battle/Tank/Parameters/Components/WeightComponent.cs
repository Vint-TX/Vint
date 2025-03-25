using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Parameters.Components;

[ProtocolId(1437571863912)]
public class WeightComponent(
    float weight
) : IComponent {
    public float Weight { get; set; } = weight;
}
