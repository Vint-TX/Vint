using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Notification;

[ProtocolId(1475754429807)]
public class UsernameChangedNotificationComponent(
    string oldUsername
) : IComponent {
    // ReSharper disable once InconsistentNaming
    public string OldUserUID { get; private set; } = oldUsername;
}