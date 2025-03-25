using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1450343100021)]
public class IncomingFriendAddedEvent(
    long userId
) : FriendAddedBaseEvent(userId);
