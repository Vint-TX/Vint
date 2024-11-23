using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450343185650)]
public class OutgoingFriendAddedEvent(
    long userId
) : FriendAddedBaseEvent(userId);
