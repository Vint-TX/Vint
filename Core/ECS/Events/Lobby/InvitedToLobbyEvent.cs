using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1499233137837)]
public class InvitedToLobbyEvent(
    string inviterUsername,
    long lobbyId,
    long engineId = 0
) : IEvent {
    [ProtocolName("userUid")] public string InviterUsername { get; private set; } = inviterUsername;
    [ProtocolName("lobbyId")] public long LobbyId { get; private set; } = lobbyId;
    [ProtocolName("engineId")] public long EngineId { get; private set; } = engineId;
}