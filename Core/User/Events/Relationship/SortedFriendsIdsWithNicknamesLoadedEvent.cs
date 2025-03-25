using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events.Relationship;

[ProtocolId(1498741007777)]
public class SortedFriendsIdsWithNicknamesLoadedEvent(
    Dictionary<long, string> friends
) : IEvent {
    public Dictionary<long, string> FriendsIdsAndNicknames { get; } = friends;
}
