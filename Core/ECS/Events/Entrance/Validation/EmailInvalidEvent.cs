using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(1455866538339)]
public class EmailInvalidEvent(
    string email
) : IEvent {
    public string Email => email;
}