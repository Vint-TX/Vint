using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Score.Events;

[ProtocolId(1463648611538)]
public class SetScoreTablePositionEvent(
    int position
) : IEvent {
    public int Position { get; private set; } = position;
}
