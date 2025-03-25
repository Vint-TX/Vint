using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.User.Events;

[ProtocolId(1469531017818)]
public class SearchUserIdByUidResultEvent(
    bool found,
    long userId
) : IEvent {
    public bool Found { get; private set; } = found;
    public long UserId { get; private set; } = userId;
}
