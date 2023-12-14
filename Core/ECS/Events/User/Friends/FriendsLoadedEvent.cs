using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

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