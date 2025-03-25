using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Lobby.State.Components;

[ProtocolId(1496833452921)]
public class MatchmakingLobbyStartTimeComponent(
    DateTimeOffset startTime
) : IComponent {
    public DateTimeOffset StartTime { get; private set; } = startTime;
}
