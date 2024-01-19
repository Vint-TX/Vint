using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Notification;

[ProtocolId(1454667308567)]
public class NotificationShownEvent : IServerEvent {
    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) { // todo
        IEntity notification = entities.Single();
        connection.Unshare(notification);
    }
}