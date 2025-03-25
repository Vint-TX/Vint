using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1496753144455)]
public class ClientExitLobbyEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InLobby) return;

        LobbyPlayer lobbyPlayer = connection.LobbyPlayer;
        LobbyBase lobby = lobbyPlayer.Lobby;
        Round? round = lobbyPlayer.Round;

        if (lobby.StateManager.CurrentState is Starting)
            return;

        if (connection.InSquad) {
            Squads.Squad squad = connection.Squad;

            if (round != null) {
                await squad.RemoveMember(connection.UserContainer.Id);
                await round.RemoveTanker(lobbyPlayer.Tanker!);

                await lobby.RemovePlayer(lobbyPlayer);
            } else if (squad.Leader != connection) {
                await squad.RemoveMember(connection.UserContainer.Id);
                await lobby.RemovePlayer(lobbyPlayer);
            } else {
                foreach (IPlayerConnection member in squad.Members)
                    await lobby.RemovePlayer(member.LobbyPlayer!);
            }
        } else {
            if (round != null)
                await round.RemoveTanker(lobbyPlayer.Tanker!);

            await lobby.RemovePlayer(lobbyPlayer);
        }
    }
}
