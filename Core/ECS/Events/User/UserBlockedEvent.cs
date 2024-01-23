using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1493022950509)]
public class UserBlockedEvent(
    string reason
) : IEvent {
    public string Reason { get; private set; } = reason;
}