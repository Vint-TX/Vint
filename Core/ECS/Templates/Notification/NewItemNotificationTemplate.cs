using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(1481176055388)]
public class NewItemNotificationTemplate : EntityTemplate {
    public IEntity Create(IEntity user, IEntity item, int amount) => Entity("notification/newitem",
        builder =>
            builder
                .AddComponent(new NewItemNotificationComponent(item, amount))
                .AddComponent(new NotificationComponent(NotificationPriority.Message))
                .AddComponent(new NotificationGroupComponent(user)));
}