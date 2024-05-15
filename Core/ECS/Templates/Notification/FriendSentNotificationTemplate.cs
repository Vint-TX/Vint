using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Notification;

[ProtocolId(1507711452261)]
public class FriendSentNotificationTemplate : NotificationTemplate {
    public IEntity Create(IEntity user) => base.Create("notification/friendSent", user);
}
