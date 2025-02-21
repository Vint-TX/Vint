using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Player;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1496753144455)]
public class ClientExitLobbyEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InLobby) return;

        LobbyPlayer lobbyPlayer = connection.LobbyPlayer;

        if (lobbyPlayer.Lobby.StateManager.CurrentState is Starting)
            return;

        if (lobbyPlayer.InRound)
            await lobbyPlayer.Round.RemoveTanker(lobbyPlayer.Tanker);

        await lobbyPlayer.Lobby.RemovePlayer(lobbyPlayer);
    }
}
