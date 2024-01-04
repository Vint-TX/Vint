using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Matchmaking;

[ProtocolId(1509109822442)]
public class ExitedFromMatchmakingEvent(
    bool selfAction
) : IEvent {
    public bool SelfAction { get; private set; } = selfAction;
}