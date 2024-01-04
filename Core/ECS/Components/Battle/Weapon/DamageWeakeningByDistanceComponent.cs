using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon;

[ProtocolId(2869455602943064305)]
public class DamageWeakeningByDistanceComponent(
    float minDamagePercent,
    float radiusOfMaxDamage,
    float radiusOfMinDamage
) : IComponent {
    public float MinDamagePercent { get; set; } = minDamagePercent;
    public float RadiusOfMaxDamage { get; set; } = radiusOfMaxDamage;
    public float RadiusOfMinDamage { get; set; } = radiusOfMinDamage;
}