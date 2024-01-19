using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450343151033)]
public class IncomingFriendRemovedEvent : FriendRemovedBaseEvent {
    public IncomingFriendRemovedEvent(long userId) => FriendId = userId;
}