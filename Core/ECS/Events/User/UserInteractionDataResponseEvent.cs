using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1412360987645)]
public class UserInteractionDataResponseEvent(
    long userId,
    string username,
    bool canRequestFriendship,
    bool friendshipRequestWasSend,
    bool muted,
    bool reported
) : IEvent {
    public long UserId { get; private set; } = userId;
    public string UserUid { get; private set; } = username;
    public bool CanRequestFriendship { get; private set; } = canRequestFriendship;
    public bool FriendshipRequestWasSend { get; private set; } = friendshipRequestWasSend;
    public bool Muted { get; private set; } = muted;
    public bool Reported { get; private set; } = reported;
}