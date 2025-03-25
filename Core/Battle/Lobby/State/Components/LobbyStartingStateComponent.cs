using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Lobby;

[ProtocolId(1499089373466)]
public class LobbyStartingStateComponent(
    DateTimeOffset startDate
) : IComponent {
    public DateTimeOffset StartDate { get; } = startDate;
}
