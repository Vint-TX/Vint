using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Movement;

[ProtocolId(1486036010735)]
public class UnitMoveRemoteEvent(
    ECS.Movement.Movement unitMove
) : UnitMoveEvent(unitMove);
