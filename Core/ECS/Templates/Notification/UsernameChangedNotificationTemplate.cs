using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(1475750208936)]
public class UsernameChangedNotificationTemplate : EntityTemplate {
    public IEntity Create(string username, IEntity user) => Entity("notification/uidchanged",
        builder =>
            builder
                .AddComponent(new UsernameChangedNotificationComponent(username))
                .AddComponent(new NotificationComponent(NotificationPriority.Message))
                .AddComponent(new NotificationGroupComponent(user)));
}