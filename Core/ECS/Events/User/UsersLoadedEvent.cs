using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1458555361768)]
public class UsersLoadedEvent(
    long requestEntityId
) : IEvent {
    public long RequestEntityId { get; private set; } = requestEntityId;
}