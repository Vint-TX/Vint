using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Parameters.Components;

[ProtocolId(1437725485852)]
public class DampingComponent(
    float damping
) : IComponent {
    public float Damping { get; set; } = damping;
}
