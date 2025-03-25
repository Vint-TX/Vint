using System.Numerics;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Damage.Events;

[ProtocolId(-4247034853035810941)]
public class CriticalDamageEvent(
    IEntity target,
    Vector3 localPosition
) : IEvent {
    public IEntity Target { get; } = target;
    public Vector3 LocalPosition { get; } = localPosition;
}
