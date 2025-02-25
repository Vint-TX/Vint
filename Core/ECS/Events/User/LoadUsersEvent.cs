using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1458555246853)]
public class LoadUsersEvent(
    GameServer server
) : IServerEvent {
    public long RequestEntityId { get; private set; }
    public HashSet<long> UsersId { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        await using DbConnection db = new();

        List<IPlayerConnection> playerConnections = server.PlayerConnections.Values
            .Where(conn => conn.IsLoggedIn)
            .ToList();

        foreach (long userId in UsersId) {
            if (!UserRegistry.TryGetContainer(userId, out UserContainer? container)) {
                Player? player = playerConnections.SingleOrDefault(conn => conn.Player.Id == userId)?.Player ??
                                 await db.Players.SingleOrDefaultAsync(player => player.Id == userId);

                if (player == null)
                    throw new InvalidOperationException($"Player {userId} not found");

                container = UserRegistry.GetOrCreateContainer(userId, player);
            }

            await container.ShareTo(connection);
        }

        await connection.Send(new UsersLoadedEvent(RequestEntityId));
    }
}
