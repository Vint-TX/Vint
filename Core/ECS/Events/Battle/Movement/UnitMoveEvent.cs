using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Movement;

[ProtocolId(1485519185293)]
public abstract class UnitMoveEvent : IEvent {
    protected UnitMoveEvent() { }

    protected UnitMoveEvent(ECS.Movement.Movement unitMove) =>
        UnitMove = unitMove;

    public ECS.Movement.Movement UnitMove { get; protected set; }
}
