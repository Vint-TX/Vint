using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Notification;

[ProtocolId(1454667308567)]
public class NotificationShownEvent : IServerEvent {
    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity notification = entities.Single();

        foreach (Server.Notification notify in connection.Notifications.Where(notify => notify.Entity == notification))
            connection.Notifications.TryRemove(notify);

        connection.Unshare(notification);
        return Task.CompletedTask;
    }
}
