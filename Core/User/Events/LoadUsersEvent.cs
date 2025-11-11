using LinqToDB.Async;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events;

[ProtocolId(1458555246853)]
public class LoadUsersEvent(
    GameServer server
) : IServerEvent {
    public long RequestEntityId { get; private set; }
    public HashSet<long> UsersId { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        await using DbConnection db = new();

        foreach (long userId in UsersId) {
            if (!UserRegistry.TryGetContainer(userId, out UserContainer? container)) {
                Player player = server.FindConnection(userId)?.Player ??
                                await db.Players.FirstAsync(player => player.Id == userId);

                container = UserRegistry.GetOrCreateContainer(userId, player);
            }

            await container.ShareTo(connection);
        }

        await connection.Send(new UsersLoadedEvent(RequestEntityId));
    }
}
