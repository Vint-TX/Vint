using Vint.Core.Battle.Tank.Movement;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Unit.Events;

[ProtocolId(1485519185293)]
public abstract class UnitMoveEvent : IEvent {
    protected UnitMoveEvent() { }

    protected UnitMoveEvent(Movement unitMove) =>
        UnitMove = unitMove;

    public Movement UnitMove { get; protected set; }
}
