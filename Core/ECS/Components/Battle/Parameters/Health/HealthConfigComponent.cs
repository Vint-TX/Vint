using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Parameters.Health;

[ProtocolId(8420700272384380156)]
public class HealthConfigComponent(
    float baseHealth
) : IComponent {
    public float BaseHealth { get; set; } = baseHealth;
}