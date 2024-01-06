using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Pause;

[ProtocolId(4633772578502170850)]
public class IdleBeginTimeSyncEvent(
    DateTimeOffset idleBeginTime
) : IEvent {
    public DateTimeOffset IdleBeginTime { get; private set; } = idleBeginTime;
}