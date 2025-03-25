using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Lobby.State.Components;

[ProtocolId(1499089373466)]
public class LobbyStartingStateComponent(
    DateTimeOffset startDate
) : IComponent {
    public DateTimeOffset StartDate { get; } = startDate;
}
