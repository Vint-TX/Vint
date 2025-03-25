using Vint.Core.ECS.Events;

namespace Vint.Core.User.Events.Relationship;

public abstract class FriendAddedRemovedBaseEvent(
    long friendId
) : IEvent {
    public long FriendId { get; private set; } = friendId;
}
