using System.Numerics;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Damage;

[ProtocolId(-4247034853035810941)]
public class CriticalDamageEvent(
    IEntity target,
    Vector3 localPosition
) : IEvent {
    public IEntity Target { get; } = target;
    public Vector3 LocalPosition { get; } = localPosition;
}