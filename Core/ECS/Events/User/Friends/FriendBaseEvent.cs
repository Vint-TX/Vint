using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450343409998)]
public abstract class FriendBaseEvent : IEvent {
    public IEntity User { get; protected set; } = null!;
}