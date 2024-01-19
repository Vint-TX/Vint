namespace Vint.Core.ECS.Events.User.Friends;

public abstract class FriendAddedRemovedBaseEvent : IEvent {
    public long FriendId { get; protected set; }
}