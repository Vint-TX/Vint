namespace Vint.Core.ECS.Events.User.Friends;

public abstract class FriendAddedBaseEvent(
    long friendId
) : FriendAddedRemovedBaseEvent(friendId);
