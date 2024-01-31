using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Flag;

[ProtocolId(2921314315544889042)]
public class FlagDropEvent(
    bool isUserAction
) : IEvent {
    public bool IsUserAction { get; private set; } = isUserAction;
}