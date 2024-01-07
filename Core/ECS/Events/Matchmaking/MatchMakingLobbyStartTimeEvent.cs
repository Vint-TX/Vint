using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1499762071035)]
public class MatchMakingLobbyStartTimeEvent(
    DateTimeOffset startTime
) : IEvent {
    public DateTimeOffset StartTime { get; private set; } = startTime;
}