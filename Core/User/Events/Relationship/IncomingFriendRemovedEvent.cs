using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1450343151033)]
public class IncomingFriendRemovedEvent(
    long userId
) : FriendRemovedBaseEvent(userId);
