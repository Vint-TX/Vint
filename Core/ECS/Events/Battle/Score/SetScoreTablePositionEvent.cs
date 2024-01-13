using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Score;

[ProtocolId(1463648611538)]
public class SetScoreTablePositionEvent(
    int position
) : IEvent {
    public int Position { get; private set; } = position;
}