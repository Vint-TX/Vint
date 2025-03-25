namespace Vint.Core.User.Events.Relationship;

public abstract class FriendRemovedBaseEvent(
    long friendId
) : FriendAddedRemovedBaseEvent(friendId);
