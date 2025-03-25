using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Flags.Events;

[ProtocolId(2921314315544889042)]
public class FlagDropEvent(
    bool isUserAction
) : IEvent {
    public bool IsUserAction { get; private set; } = isUserAction;
}
