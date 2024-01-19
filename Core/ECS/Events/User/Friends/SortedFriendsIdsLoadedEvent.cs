using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1457951948522)]
public class SortedFriendsIdsLoadedEvent(
    Dictionary<long, string> friends,
    Dictionary<long, string> incoming,
    Dictionary<long, string> outgoing
) : IEvent {
    public Dictionary<long, string> FriendsAcceptedIds { get; private set; } = friends;
    public Dictionary<long, string> FriendsIncomingIds { get; private set; } = incoming;
    public Dictionary<long, string> FriendsOutgoingIds { get; private set; } = outgoing;
}