using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1450343296915)]
public class AcceptedFriendRemovedEvent(
    long userId
) : FriendRemovedBaseEvent(userId);
