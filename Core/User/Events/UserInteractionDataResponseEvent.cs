using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.User;

[ProtocolId(1412360987645)]
public class UserInteractionDataResponseEvent : IEvent {
    public required long UserId { get; init; }
    public required string Username { get; init; }
    public required bool CanRequestFriendship { get; init; }
    public required bool FriendshipRequestWasSend { get; init; }
    public required bool Blocked { get; init; }
    public required bool Reported { get; init; }
}
