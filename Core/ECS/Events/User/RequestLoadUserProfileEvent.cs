using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1451368548585)]
public class RequestLoadUserProfileEvent : IServerEvent {
    public long UserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        if (!UserRegistry.TryGetContainer(UserId, out UserContainer? container)) {
            GameServer server = serviceProvider.GetRequiredService<GameServer>();
            await using DbConnection db = new();

            Player? player = server.PlayerConnections.Values.Where(conn => conn.IsOnline).SingleOrDefault(conn => conn.Player.Id == UserId)?.Player ??
                             await db.Players.SingleOrDefaultAsync(player => player.Id == UserId);

            if (player == null)
                throw new InvalidOperationException($"Player {UserId} not found");

            container = UserRegistry.GetOrCreateContainer(UserId, player);
        }

        await connection.Share(container.Entity);
        await connection.Send(new UserProfileLoadedEvent(), container.Entity);
    }
}
