using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450264928332)]
public class BreakOffFriendEvent(
    GameServer server
) : FriendBaseEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        long senderId = connection.UserContainer.Id;

        await using DbConnection db = new();

        bool success = await db.Friends
            .Where(friend => (friend.UserId == senderId && friend.FriendId == UserId) ||
                             (friend.UserId == UserId && friend.FriendId == senderId))
            .DeleteAsync() > 0;

        if (!success) return;

        await connection.Send(new AcceptedFriendRemovedEvent(UserId), connection.UserContainer.Entity);

        IPlayerConnection? targetConnection = server.FindConnection(UserId);

        if (targetConnection != null)
            await targetConnection.Send(new AcceptedFriendRemovedEvent(senderId), UserContainer.Entity);
    }
}
