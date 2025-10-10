using Vint.Core.Battle.Lobby.State;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Squads;

namespace Vint.Core.Battle.Lobby.Events;

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
            Squad squad = connection.Squad;

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
            if (lobbyPlayer.InRound)
                await round!.RemoveTanker(lobbyPlayer.Tanker);

            await lobby.RemovePlayer(lobbyPlayer);
        }
    }
}
