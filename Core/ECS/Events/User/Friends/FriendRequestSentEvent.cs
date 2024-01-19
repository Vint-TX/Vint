using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Notification;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1507708322833)]
public class FriendRequestSentEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        connection.Share(new FriendSentNotificationTemplate().Create(entities.Single()));
        connection.Send(new ShowNotificationGroupEvent(1));
    }
}