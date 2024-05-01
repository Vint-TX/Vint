using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497002374017)]
public class InviteToLobbyEvent : IServerEvent {
    public long[] InvitedUsersIds { get; private set; } = null!;

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return Task.CompletedTask;

        List<IPlayerConnection> connections = connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .ToList();

        foreach (IPlayerConnection? receiver in InvitedUsersIds.Select(userId => connections.SingleOrDefault(conn => conn.Player.Id == userId)))
            receiver?.Send(new InvitedToLobbyEvent(connection.Player.Username, connection.BattlePlayer!.Battle.LobbyId), receiver.User);

        return Task.CompletedTask;
    }
}
