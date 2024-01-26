using System.Numerics;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Damage;

[ProtocolId(1494934093730)]
public class DamageInfoEvent(
    Vector3 hitPoint,
    float damage,
    bool isCritical,
    bool isHealHit = false
) : IEvent {
    public Vector3 HitPoint { get; } = hitPoint;
    public float Damage { get; } = damage;
    [ProtocolName("BackHit")] public bool Critical { get; } = isCritical;
    public bool HealHit { get; } = isHealHit;
}