using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.User;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Events.User.Friends;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1507198221820)]
public class ChangeBlockStateByUserIdRequestEvent(
    GameServer server
) : IServerEvent {
    public InteractionSource InteractionSource { get; set; }
    public long SourceId { get; set; }
    [ProtocolName("UserId")] public long BlockedUserId { get; set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        await using DbConnection db = new();
        bool targetPlayerExists = await db.Players.AnyAsync(player => player.Id == BlockedUserId);

        if (!targetPlayerExists) return;

        long blockerUserId = connection.UserContainer.Id;

        await db.BeginTransactionAsync();

        bool needToBlock = await db.Blocks
            .Where(block => block.BlockerId == blockerUserId &&
                            block.BlockedId == BlockedUserId)
            .DeleteAsync() == 0;

        IPlayerConnection? blockedConnection = null;
        bool friendshipBroken = false;
        bool incomingRequestRejected = false;
        bool outgoingRequestRemoved = false;

        if (needToBlock) {
            blockedConnection = server.FindConnection(BlockedUserId);

            Block block = new() {
                BlockerId = blockerUserId,
                BlockedId = BlockedUserId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await db.InsertAsync(block);

            friendshipBroken = await db.Friends
                .Where(friend => (friend.UserId == blockerUserId && friend.FriendId == BlockedUserId) ||
                                 (friend.UserId == BlockedUserId && friend.FriendId == blockerUserId))
                .DeleteAsync() > 0;

            incomingRequestRejected = await db.FriendRequests
                .Where(request => request.SenderId == BlockedUserId && request.FriendId == blockerUserId)
                .DeleteAsync() > 0;

            outgoingRequestRemoved = await db.FriendRequests
                .Where(request => request.SenderId == blockerUserId && request.FriendId == BlockedUserId)
                .DeleteAsync() > 0;
        }

        await db.CommitTransactionAsync();

        await connection.UserContainer.Entity.ChangeComponent<BlackListComponent>(component => component.BlockedUsers.Add(BlockedUserId));

        if (friendshipBroken) {
            await connection.Send(new AcceptedFriendRemovedEvent(BlockedUserId), connection.UserContainer.Entity);

            if (blockedConnection != null)
                await blockedConnection.Send(new AcceptedFriendRemovedEvent(blockerUserId), blockedConnection.UserContainer.Entity);
        }

        if (incomingRequestRejected) {
            await connection.Send(new IncomingFriendRemovedEvent(BlockedUserId), connection.UserContainer.Entity);

            if (blockedConnection != null)
                await blockedConnection.Send(new OutgoingFriendRemovedEvent(blockerUserId), blockedConnection.UserContainer.Entity);
        }

        if (outgoingRequestRemoved) {
            await connection.Send(new OutgoingFriendRemovedEvent(BlockedUserId), connection.UserContainer.Entity);

            if (blockedConnection != null)
                await blockedConnection.Send(new IncomingFriendRemovedEvent(blockerUserId), blockedConnection.UserContainer.Entity);
        }
    }
}
