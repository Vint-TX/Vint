using Vint.Core.Battle.Tank.Movement;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Unit.Events;

[ProtocolId(1486036010735)]
public class UnitMoveRemoteEvent(
    Movement unitMove
) : UnitMoveEvent(unitMove);
