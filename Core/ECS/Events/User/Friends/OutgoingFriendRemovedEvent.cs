using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450343225471)]
public class OutgoingFriendRemovedEvent : FriendRemovedBaseEvent {
    public OutgoingFriendRemovedEvent(long userId) => FriendId = userId;
}