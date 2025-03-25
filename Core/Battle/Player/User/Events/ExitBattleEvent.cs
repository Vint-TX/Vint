using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.User.Events;

[ProtocolId(-4669704207166218448)]
public class ExitBattleEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.Spectating)
            await ExitFromRound(connection.Spectator);
        else {
            LobbyPlayer lobbyPlayer = connection.LobbyPlayer!;

            if (connection.InLobby && lobbyPlayer.InRound) {
                if (connection.InSquad && lobbyPlayer.Tanker.Round.StateManager.CurrentState is not Ended)
                    await connection.Squad.RemoveMember(connection.UserContainer.Id);

                await ExitFromRound(lobbyPlayer);
            }
        }
    }

    static async Task ExitFromRound(Spectator spectator) =>
        await spectator.Round.RemoveSpectator(spectator);

    static async Task ExitFromRound(LobbyPlayer lobbyPlayer) {
        Tanker tanker = lobbyPlayer.Tanker!;
        Round round = tanker.Round;

        await round.RemoveTanker(tanker);
    }
}
