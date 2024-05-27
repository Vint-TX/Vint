using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.User;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1451368548585)]
public class RequestLoadUserProfileEvent : IServerEvent {
    public long UserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity? user = connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == UserId)?.User;

        if (EntityRegistry.TryGetTemp(UserId, out IEntity? tempUser)) { // temp user exists..
            if (user != null) { // ..but player is online
                foreach (IPlayerConnection shared in tempUser.SharedPlayers) {
                    await shared.Unshare(tempUser);
                    await shared.Share(user);
                }

                await connection.ShareIfUnshared(user);
                EntityRegistry.TryRemoveTemp(UserId);
            } else { // ..and player is offline
                await connection.ShareIfUnshared(tempUser);
                user = tempUser;
            }
        } else if (user != null) { // player is online
            await connection.ShareIfUnshared(user);
        } else { // player is offline
            await using DbConnection db = new();
            Player? player = await db.Players.SingleOrDefaultAsync(player => player.Id == UserId);

            if (player == null) return;

            user = new UserTemplate().CreateFake(connection, player);
            await connection.Share(user);
        }

        await connection.Send(new UserProfileLoadedEvent(), user);
    }
}
