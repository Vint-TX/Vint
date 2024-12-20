using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497002374017)]
public class InviteToLobbyEvent(
    GameServer server
) : IServerEvent {
    public long[] InvitedUsersIds { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.InLobby) return;

        List<IPlayerConnection> connections = server
            .PlayerConnections
            .Values
            .Where(conn => conn.IsOnline)
            .ToList();

        foreach (IPlayerConnection receiver in InvitedUsersIds
                     .Select(userId => connections.SingleOrDefault(conn => conn.Player.Id == userId))
                     .OfType<IPlayerConnection>())
            await receiver.Send(new InvitedToLobbyEvent(connection.Player.Username, connection.BattlePlayer!.Battle.LobbyId), receiver.UserContainer.Entity);
    }
}
