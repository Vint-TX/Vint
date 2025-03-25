using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1450168255217)]
public class AcceptFriendEvent(
    GameServer server
) : FriendBaseEvent, IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        long receiverId = connection.UserContainer.Id;

        await using DbConnection db = new();
        await db.BeginTransactionAsync();

        FriendRequest? request = (await db.FriendRequests
            .Where(request => request.SenderId == UserId && request.FriendId == receiverId)
            .DeleteWithOutputAsync()
            .ToListAsync())
            .SingleOrDefault();

        if (request == null) return;

        Friend senderToReceiver = new() {
            UserId = UserId,
            FriendId = receiverId,
            RequestedAt = request.CreatedAt,
            AcceptedAt = DateTimeOffset.UtcNow
        };

        Friend receiverToSender = new() {
            UserId = receiverId,
            FriendId = UserId,
            RequestedAt = request.CreatedAt,
            AcceptedAt = DateTimeOffset.UtcNow
        };

        await db.InsertAsync(senderToReceiver);
        await db.InsertAsync(receiverToSender);

        await db.CommitTransactionAsync();

        await connection.Send(new IncomingFriendRemovedEvent(UserId), connection.UserContainer.Entity);
        await connection.Send(new AcceptedFriendAddedEvent(UserId), connection.UserContainer.Entity);

        IPlayerConnection? sender = server.FindConnection(UserId);

        if (sender == null) return;

        await sender.Send(new OutgoingFriendRemovedEvent(receiverId), UserContainer.Entity);
        await sender.Send(new AcceptedFriendAddedEvent(receiverId), UserContainer.Entity);
    }
}
