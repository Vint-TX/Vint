using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Matchmaking.Events;

[ProtocolId(1499762071035)]
public class MatchMakingLobbyStartTimeEvent(
    DateTimeOffset startTime
) : IEvent {
    public DateTimeOffset StartTime { get; private set; } = startTime;
}
