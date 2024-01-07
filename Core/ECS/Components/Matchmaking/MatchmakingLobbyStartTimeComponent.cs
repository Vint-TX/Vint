using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Matchmaking;

[ProtocolId(1496833452921)]
public class MatchmakingLobbyStartTimeComponent(
    DateTimeOffset startTime
) : IComponent {
    public DateTimeOffset StartTime { get; private set; } = startTime;
}