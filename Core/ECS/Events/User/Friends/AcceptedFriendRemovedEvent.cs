using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450343296915)]
public class AcceptedFriendRemovedEvent : FriendRemovedBaseEvent {
    public AcceptedFriendRemovedEvent(long userId) => FriendId = userId;
}