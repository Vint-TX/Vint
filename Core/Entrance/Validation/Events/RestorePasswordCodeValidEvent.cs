using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Validation.Events;

[ProtocolId(1460402875430)]
public class RestorePasswordCodeValidEvent(
    string code
) : IEvent {
    public string Code => code;
}
