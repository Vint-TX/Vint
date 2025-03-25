using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Squads.Events;

[ProtocolId(1507799564788)]
public class RequestedToSquadEvent(
    string userUid,
    long fromUserId,
    long squadId
) : IEvent {
    public string UserUid { get; } = userUid;
    public long FromUserId { get; } = fromUserId;
    public long SquadId { get; } = squadId;
}
