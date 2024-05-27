using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450343273642)]
public class AcceptedFriendAddedEvent(
    long userId
) : FriendAddedBaseEvent(userId);
