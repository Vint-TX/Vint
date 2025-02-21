using Vint.Core.Battle.Lobby;
using Vint.Core.Battle.Matchmaking;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1495176527022)]
public class ExitFromMatchmakingEvent(
    RatingMatchmakingProcessor rating,
    ArcadeMatchmakingProcessor arcade
) : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.InLobby && connection.LobbyPlayer.Lobby.StateManager.CurrentState is Starting)
            return;

        await rating.TryDequeuePlayer(connection); // fkin bruh
        await arcade.TryDequeuePlayer(connection);

        await connection.Send(new ExitedFromMatchmakingEvent(true), entities);
    }
}
