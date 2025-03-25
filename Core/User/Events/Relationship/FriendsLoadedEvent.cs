using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1451120695251)]
public class FriendsLoadedEvent(
    HashSet<long> acceptedFriendIds,
    HashSet<long> incomingFriendIds,
    HashSet<long> outgoingFriendIds
) : IEvent {
    public HashSet<long> AcceptedFriendIds => acceptedFriendIds;
    public HashSet<long> IncomingFriendIds => incomingFriendIds;
    public HashSet<long> OutgoingFriendIds => outgoingFriendIds;
}
