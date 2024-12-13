using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1458555246853)]
public class LoadUsersEvent : IServerEvent {
    public long RequestEntityId { get; private set; }
    public HashSet<long> UsersId { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        await using DbConnection db = new();
        GameServer server = serviceProvider.GetRequiredService<GameServer>();

        List<IPlayerConnection> playerConnections = server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .ToList();

        foreach (long userId in UsersId) {
            if (UserRegistry.TryGetContainer(userId, out UserContainer? container)) {
                await connection.ShareIfUnshared(container.Entity);
                continue;
            }

            Player? player = playerConnections.SingleOrDefault(conn => conn.Player.Id == userId)?.Player ??
                             await db.Players.SingleOrDefaultAsync(player => player.Id == userId);

            if (player == null)
                throw new InvalidOperationException($"Player {userId} not found");

            await connection.ShareIfUnshared(UserRegistry.GetOrCreateContainer(userId, player).Entity);
        }

        await connection.Send(new UsersLoadedEvent(RequestEntityId));
    }
}
