using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Railgun;

[ProtocolId(6707178642658066560)]
public class DamageWeakeningByTargetComponent(
    float damagePercent
) : IComponent {
    public float DamagePercent { get; set; } = damagePercent;
}
