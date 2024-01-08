using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1547709389410)]
public class EnterBattleLobbyFailedEvent(
    bool alreadyInLobby,
    bool lobbyIsFull
) : IEvent {
    public bool AlreadyInLobby { get; private set; } = alreadyInLobby;
    public bool LobbyIsFull { get; private set; } = lobbyIsFull;
}