using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Parameters.Components;

[ProtocolId(8420700272384380156)]
public class HealthConfigComponent(
    float baseHealth
) : IComponent {
    public float BaseHealth { get; set; } = baseHealth;
}
