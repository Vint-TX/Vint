using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1450343273642)]
public class AcceptedFriendAddedEvent(
    long userId
) : FriendAddedBaseEvent(userId);
