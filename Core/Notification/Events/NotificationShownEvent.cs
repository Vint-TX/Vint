using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Notification.Components;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Notification.Events;

[ProtocolId(1454667308567)]
public class NotificationShownEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity notification = entities.Single();

        if (!notification.HasComponent<NotificationComponent>()) {
            connection.Logger.ForType<NotificationShownEvent>().Error("Entity does not have NotificationComponent: {Entity}", notification);
            return;
        }

        await connection.Unshare(notification);
    }
}
