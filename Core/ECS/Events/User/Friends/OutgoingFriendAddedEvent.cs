using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450343185650)]
public class OutgoingFriendAddedEvent : FriendAddedBaseEvent {
    public OutgoingFriendAddedEvent(long userId) => FriendId = userId;
}