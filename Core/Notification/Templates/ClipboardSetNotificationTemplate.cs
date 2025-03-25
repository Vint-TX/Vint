using Vint.Core.ECS.Entities;
using Vint.Core.Notification.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Notification.Templates;

[ProtocolId(31219)]
public class ClipboardSetNotificationTemplate : NotificationTemplate {
    public IEntity Create(IEntity user) {
        IEntity entity = base.Create("notification/clipboardSet", user);

        entity.AddComponent<ClipboardSetNotificationComponent>();
        return entity;
    }
}
