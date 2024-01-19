using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Notification;

[ProtocolId(1464339267328)]
public class NotificationComponent(
    NotificationPriority priority,
    DateTimeOffset? timeCreation = null
) : IComponent {
    public NotificationPriority Priority { get; set; } = priority;
    public DateTimeOffset TimeCreation { get; set; } = timeCreation ?? DateTimeOffset.UtcNow;
}