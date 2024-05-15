using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(1493196797791)]
public class SimpleTextNotificationTemplate : NotificationTemplate {
    public IEntity Create(string message) {
        IEntity entity = base.Create("notification/simpletext");

        entity.AddComponent(new ServerNotificationMessageComponent(message));
        return entity;
    }
}
