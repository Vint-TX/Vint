using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1496829083447)]
public class MatchMakingUserReadyEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InLobby)
            return;

        LobbyPlayer lobbyPlayer = connection.LobbyPlayer;

        if (lobbyPlayer.InRound || lobbyPlayer.Ready)
            return;

        await lobbyPlayer.Lobby.PlayerReady(lobbyPlayer);
    }
}
