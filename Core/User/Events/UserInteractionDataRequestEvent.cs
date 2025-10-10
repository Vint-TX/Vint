using LinqToDB;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.User.Events;

[ProtocolId(1454623211245)]
public class UserInteractionDataRequestEvent : IServerEvent {
    public long UserId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        long requesterId = connection.UserContainer.Id;

        if (requesterId == UserId || !UserRegistry.TryGetContainer(UserId, out UserContainer? container))
            return;

        string username = container.Entity.GetComponent<UserUidComponent>().Username;
        bool isFriends;
        bool isBlocked;
        bool isReported;
        bool isOutgoingRequestSent;

        await using (DbConnection db = new()) {
            isFriends = await db.Friends.AnyAsync(friend => friend.UserId == requesterId && friend.FriendId == UserId);
            isBlocked = await db.Blocks.AnyAsync(block => block.BlockerId == requesterId && block.BlockedId == UserId);
            isReported = await db.Reports.AnyAsync(report => report.ReporterId == requesterId && report.ReportedId == UserId);
            isOutgoingRequestSent = await db.FriendRequests.AnyAsync(request => request.SenderId == requesterId && request.FriendId == UserId);
        }

        await connection.Send(new UserInteractionDataResponseEvent {
            UserId = UserId,
            Username = username,
            CanRequestFriendship = !isFriends && !isOutgoingRequestSent,
            FriendshipRequestWasSend = isOutgoingRequestSent,
            Blocked = isBlocked,
            Reported = isReported
        }, connection.UserContainer.Entity);
    }
}
