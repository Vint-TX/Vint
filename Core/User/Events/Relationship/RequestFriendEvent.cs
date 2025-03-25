using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Notification.Templates;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1450168139800)]
public class RequestFriendEvent(
    GameServer server
) : FriendBaseEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        long senderId = connection.UserContainer.Id;

        if (senderId == UserId) return;

        await using DbConnection db = new();
        bool canRequestFriend = !await AlreadyRequested(db, senderId, UserId) &&
                                !await AlreadyFriends(db, senderId, UserId) &&
                                !await SenderBlocked(db, senderId, UserId);

        if (!canRequestFriend) return;

        await db.BeginTransactionAsync();
        await db.Blocks
            .Where(block => block.BlockerId == senderId && block.BlockedId == UserId)
            .DeleteAsync();

        FriendRequest request = new() {
            SenderId = senderId,
            FriendId = UserId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        request.Id = await db.InsertWithInt32IdentityAsync(request);

        await db.CommitTransactionAsync();

        await connection.Send(new OutgoingFriendAddedEvent(UserId), connection.UserContainer.Entity);
        await connection.Share(new FriendSentNotificationTemplate().Create(connection.UserContainer.Entity));

        IPlayerConnection? targetConnection = server.FindConnection(UserId);

        if (targetConnection != null)
            await targetConnection.Send(new IncomingFriendAddedEvent(senderId), UserContainer.Entity);
    }

    static Task<bool> AlreadyFriends(DbConnection db, long senderId, long receiverId) => db.Friends
        .AnyAsync(friend => (friend.UserId == senderId && friend.FriendId == receiverId) ||
                            (friend.UserId == receiverId && friend.FriendId == senderId));
    static Task<bool> AlreadyRequested(DbConnection db, long senderId, long receiverId) => db.FriendRequests
        .AnyAsync(request => (request.SenderId == senderId && request.FriendId == receiverId) ||
                             (request.SenderId == receiverId && request.FriendId == senderId));

    static Task<bool> SenderBlocked(DbConnection db, long senderId, long receiverId) => db.Blocks
        .AnyAsync(block => block.BlockerId == receiverId && block.BlockedId == senderId);
}
