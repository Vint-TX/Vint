using Vint.Core.Battle.Player;
using Vint.Core.Battle.Rounds;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(-4669704207166218448)]
public class ExitBattleEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.Spectating)
            await ExitFromRound(connection.Spectator);
        else if (connection.InLobby && connection.LobbyPlayer.InRound)
            await ExitFromRound(connection.LobbyPlayer);
    }

    static async Task ExitFromRound(Spectator spectator) =>
        await spectator.Round.RemoveSpectator(spectator);

    static async Task ExitFromRound(LobbyPlayer lobbyPlayer) {
        Tanker tanker = lobbyPlayer.Tanker!;
        Round round = tanker.Round;

        await round.RemoveTanker(tanker);
    }
}
