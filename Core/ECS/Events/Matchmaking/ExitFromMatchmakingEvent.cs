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

        if (connection.InSquad) {
            Squads.Squad squad = connection.Squad;

            if (squad.Leader == connection) {
                foreach (IPlayerConnection member in squad.Members.Where(member => member != connection)) {
                    await TryDequeue(member);
                    await member.Send(new ExitedFromMatchmakingEvent(false), [..entities, member.UserContainer.Entity]);
                }
            } else
                await squad.RemoveMember(connection.UserContainer.Id);
        }

        await TryDequeue(connection);
        await connection.Send(new ExitedFromMatchmakingEvent(true), [..entities, connection.UserContainer.Entity]);
    }

    async Task TryDequeue(IPlayerConnection connection) {
        await rating.TryDequeuePlayer(connection);
        await arcade.TryDequeuePlayer(connection);
    }
}
