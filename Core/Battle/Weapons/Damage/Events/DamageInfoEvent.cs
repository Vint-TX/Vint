using System.Numerics;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Damage.Events;

[ProtocolId(1494934093730)]
public class DamageInfoEvent(
    Vector3 hitPoint,
    float damage,
    bool backHit,
    bool isHealHit = false
) : IEvent {
    public Vector3 HitPoint { get; } = hitPoint;
    public float Damage { get; } = damage;
    public bool BackHit { get; } = backHit;
    public bool HealHit { get; } = isHealHit;
}
