using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497349612322)]
public class AcceptInviteEvent(
    LobbyProcessor lobbyProcessor
) : IServerEvent {
    [ProtocolName("lobbyId")] public long LobbyId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        LobbyBase? lobby = lobbyProcessor.FindByLobbyId(LobbyId);

        if (lobby == null)
            return;

        if (connection.InLobby) {
            LobbyPlayer lobbyPlayer = connection.LobbyPlayer;

            if (lobbyPlayer.InRound)
                await lobbyPlayer.Round.RemoveTanker(lobbyPlayer.Tanker);

            await lobbyPlayer.Lobby.RemovePlayer(lobbyPlayer);
        }

        await lobby.AddPlayer(connection);
    }
}
