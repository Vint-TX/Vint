using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Components.Notification;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(1507711452261)]
public class FriendSentNotificationTemplate : EntityTemplate {
    public IEntity Create(IEntity user) => Entity("notification/friendSent",
        builder => builder
            .AddGroupComponent<NotificationGroupComponent>(user)
            .AddComponent(new NotificationComponent(NotificationPriority.Message)));
}