using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon;

[ProtocolId(7115193786389139467)]
public class WeaponCooldownComponent(
    float cooldownIntervalSec
) : IComponent {
    public float CooldownIntervalSec { get; set; } = cooldownIntervalSec;
}