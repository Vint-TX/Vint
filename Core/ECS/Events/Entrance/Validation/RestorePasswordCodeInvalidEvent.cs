using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(1460402823575)]
public class RestorePasswordCodeInvalidEvent(
    string code
) : IEvent {
    public string Code => code;
}