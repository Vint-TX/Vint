using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1450168274692)]
public class RejectFriendEvent(
    GameServer server
) : FriendBaseEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        long receiverId = connection.UserContainer.Id;
        await using DbConnection db = new();

        bool success = await db.FriendRequests
            .Where(request => request.SenderId == UserId && request.FriendId == receiverId)
            .DeleteAsync() > 0;

        if (!success) return;

        await connection.Send(new IncomingFriendRemovedEvent(UserId), connection.UserContainer.Entity);

        IPlayerConnection? targetConnection = server.FindConnection(UserId);

        if (targetConnection != null)
            await targetConnection.Send(new OutgoingFriendRemovedEvent(receiverId), UserContainer.Entity);
    }
}
