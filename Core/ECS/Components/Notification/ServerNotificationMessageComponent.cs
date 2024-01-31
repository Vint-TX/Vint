using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Notification;

[ProtocolId(1493197354957)]
public class ServerNotificationMessageComponent(
    string message
) : IComponent {
    public string Message { get; private set; } = message;
}