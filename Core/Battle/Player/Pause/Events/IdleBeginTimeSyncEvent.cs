using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.Pause.Events;

[ProtocolId(4633772578502170850)]
public class IdleBeginTimeSyncEvent(
    DateTimeOffset idleBeginTime
) : IEvent {
    public DateTimeOffset IdleBeginTime { get; private set; } = idleBeginTime;
}
