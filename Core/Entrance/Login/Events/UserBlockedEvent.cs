using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Login.Events;

[ProtocolId(1493022950509)]
public class UserBlockedEvent(
    string reason
) : IEvent {
    public string Reason { get; private set; } = reason;
}
