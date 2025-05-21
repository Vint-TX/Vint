using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.RestorePassword.Events;

[ProtocolId(1460402823575)]
public class RestorePasswordCodeInvalidEvent(
    string code
) : IEvent {
    public string Code => code;
}
