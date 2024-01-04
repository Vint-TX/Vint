using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Parameters.Health;

[ProtocolId(1949198098578360952)]
public class HealthComponent(
    float currentHealth,
    float maxHealth
) : IComponent {
    public float CurrentHealth { get; set; } = currentHealth;
    public float MaxHealth { get; set; } = maxHealth;
}