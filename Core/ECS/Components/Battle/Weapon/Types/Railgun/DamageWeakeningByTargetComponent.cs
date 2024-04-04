using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Railgun;

[ProtocolId(6707178642658066560)]
public class DamageWeakeningByTargetComponent(
    float damagePercent
) : IComponent {
    public float DamagePercent { get; set; } = damagePercent;
}