namespace Vint.Core.ECS.Events.User.Friends;

public abstract class FriendRemovedBaseEvent(
    long friendId
) : FriendAddedRemovedBaseEvent(friendId);
