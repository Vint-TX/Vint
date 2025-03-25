using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1450343185650)]
public class OutgoingFriendAddedEvent(
    long userId
) : FriendAddedBaseEvent(userId);
