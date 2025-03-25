using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User.Friends;

[ProtocolId(1498741007777)]
public class SortedFriendsIdsWithNicknamesLoadedEvent(
    Dictionary<long, string> friends
) : IEvent {
    public Dictionary<long, string> FriendsIdsAndNicknames { get; } = friends;
}
