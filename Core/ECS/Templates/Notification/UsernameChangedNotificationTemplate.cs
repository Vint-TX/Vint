using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(1475750208936)]
public class UsernameChangedNotificationTemplate : NotificationTemplate {
    public new IEntity Create(string username, IEntity user) {
        IEntity entity = base.Create("notification/uidchanged", user);

        entity.AddComponent(new UsernameChangedNotificationComponent(username));
        return entity;
    }
}
