using Vint.Core.ECS.Entities;
using Vint.Core.Notification.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Notification.Templates;

[ProtocolId(1475750208936)]
public class UsernameChangedNotificationTemplate : NotificationTemplate {
    public new IEntity Create(string username, IEntity user) {
        IEntity entity = base.Create("notification/uidchanged", user);

        entity.AddComponent(new UsernameChangedNotificationComponent(username));
        return entity;
    }
}
