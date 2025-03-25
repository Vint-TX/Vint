using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Parameters.Components;

[ProtocolId(1949198098578360952)]
public class HealthComponent(
    float currentHealth,
    float maxHealth
) : IComponent {
    public float CurrentHealth { get; set; } = currentHealth;
    public float MaxHealth { get; set; } = maxHealth;
}
