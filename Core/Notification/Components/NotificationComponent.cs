using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Notification.Components;

[ProtocolId(1464339267328)]
public class NotificationComponent(
    NotificationPriority priority,
    DateTimeOffset? timeCreation = null
) : IComponent {
    public NotificationPriority Priority { get; set; } = priority;
    public DateTimeOffset TimeCreation { get; set; } = timeCreation ?? DateTimeOffset.UtcNow;
}
