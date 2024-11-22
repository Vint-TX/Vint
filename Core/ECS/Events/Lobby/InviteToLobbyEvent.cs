using Microsoft.Extensions.DependencyInjection;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server.Game;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497002374017)]
public class InviteToLobbyEvent : IServerEvent {
    public long[] InvitedUsersIds { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (!connection.InLobby) return;

        GameServer server = serviceProvider.GetRequiredService<GameServer>();

        List<IPlayerConnection> connections = server
            .PlayerConnections
            .Values
            .Where(conn => conn.IsOnline)
            .ToList();

        foreach (IPlayerConnection receiver in InvitedUsersIds
                     .Select(userId => connections.SingleOrDefault(conn => conn.Player.Id == userId))
                     .OfType<IPlayerConnection>())
            await receiver.Send(new InvitedToLobbyEvent(connection.Player.Username, connection.BattlePlayer!.Battle.LobbyId), receiver.User);
    }
}
