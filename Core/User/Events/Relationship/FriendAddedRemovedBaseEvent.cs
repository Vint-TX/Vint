namespace Vint.Core.ECS.Events.User.Friends;

public abstract class FriendAddedRemovedBaseEvent(
    long friendId
) : IEvent {
    public long FriendId { get; private set; } = friendId;
}
