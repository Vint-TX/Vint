using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(1460402875430)]
public class RestorePasswordCodeValidEvent(
    string code
) : IEvent {
    public string Code => code;
}