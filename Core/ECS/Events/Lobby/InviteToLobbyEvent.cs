using Vint.Core.Battle.Lobby.Impl;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1497002374017)]
public class InviteToLobbyEvent(
    GameServer server
) : IServerEvent {
    public long InvitedUserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (connection.LobbyPlayer?.Lobby is not CustomLobby lobby)
            return;

        IPlayerConnection? target = server.PlayerConnections.Values
            .Where(conn => conn.IsLoggedIn)
            .FirstOrDefault(conn => conn.Player.Id == InvitedUserId);

        if (target == null)
            return;

        await target.Send(new InvitedToLobbyEvent(connection.Player.Username, lobby.Entity.Id), target.UserContainer.Entity);
    }
}
