using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450343100021)]
public class IncomingFriendAddedEvent(
    long userId
) : FriendAddedBaseEvent(userId);
