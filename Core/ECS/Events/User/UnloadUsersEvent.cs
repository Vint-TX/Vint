using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1458555309592)]
public class UnloadUsersEvent : IServerEvent {
    public HashSet<long> Users { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        // if (connection.InLobby)
        //     Users = Users.Except(connection.LobbyPlayer.Lobby.Players.Select(player => player.Connection.UserContainer.Id)).ToHashSet();

        foreach (UserContainer container in Users.Select(UserRegistry.GetContainer)) {
            await container.UnshareFrom(connection);
        }
    }
}
