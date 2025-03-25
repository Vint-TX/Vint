using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1450343225471)]
public class OutgoingFriendRemovedEvent(
    long userId
) : FriendRemovedBaseEvent(userId);
