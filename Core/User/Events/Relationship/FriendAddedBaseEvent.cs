namespace Vint.Core.User.Events.Relationship;

public abstract class FriendAddedBaseEvent(
    long friendId
) : FriendAddedRemovedBaseEvent(friendId);
