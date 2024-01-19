using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450343100021)]
public class IncomingFriendAddedEvent : FriendAddedBaseEvent {
    public IncomingFriendAddedEvent(long userId) => FriendId = userId;
}