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

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity? user = connection.Server.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Id == UserId)?.User;

        if (EntityRegistry.TryGetTemp(UserId, out IEntity? tempUser)) { // temp user exists..
            if (user != null) { // ..but player is online
                foreach (IPlayerConnection shared in tempUser.SharedPlayers) {
                    shared.Unshare(tempUser);
                    shared.Share(user);
                }

                connection.ShareIfUnshared(user);
                EntityRegistry.TryRemoveTemp(UserId);
            } else { // ..and player is offline
                connection.ShareIfUnshared(tempUser);
                user = tempUser;
            }
        } else if (user != null) { // player is online
            connection.ShareIfUnshared(user);
        } else { // player is offline
            using DbConnection db = new();
            Player? player = db.Players.SingleOrDefault(player => player.Id == UserId);

            if (player == null) return;

            user = new UserTemplate().CreateFake(connection, player);
            connection.Share(user);
        }

        connection.Send(new UserProfileLoadedEvent(), user);
    }
}