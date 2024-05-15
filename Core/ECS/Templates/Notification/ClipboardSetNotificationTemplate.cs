using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(31219)]
public class ClipboardSetNotificationTemplate : NotificationTemplate {
    public IEntity Create(IEntity user) {
        IEntity entity = base.Create("notification/clipboardSet", user);

        entity.AddComponent<ClipboardSetNotificationComponent>();
        return entity;
    }
}
