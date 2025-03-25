using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Matchmaking.Events;

[ProtocolId(1509109822442)]
public class ExitedFromMatchmakingEvent(
    bool selfAction
) : IEvent {
    public bool SelfAction { get; private set; } = selfAction;
}
