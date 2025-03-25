using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events;

[ProtocolId(1458555361768)]
public class UsersLoadedEvent(
    long requestEntityId
) : IEvent {
    public long RequestEntityId { get; private set; } = requestEntityId;
}
