using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1507543176898)]
public class InvitedToSquadEvent(
    string username,
    long fromUserId
) : IEvent {
    public string Username { get; } = username;
    public long FromUserId { get; } = fromUserId;
}
