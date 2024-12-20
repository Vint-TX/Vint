using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Notification;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1507708322833)]
public class FriendRequestSentEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        await connection.Share(new FriendSentNotificationTemplate().Create(entities.Single()));
        await connection.Send(new ShowNotificationGroupEvent());
    }
}
