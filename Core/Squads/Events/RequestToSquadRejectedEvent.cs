using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1508315556885)]
public class RequestToSquadRejectedEvent(
    RejectRequestToSquadReason reason,
    long requestReceiverId
) : IEvent {
    public RejectRequestToSquadReason Reason { get; } = reason;
    public long RequestReceiverId { get; } = requestReceiverId;
}
