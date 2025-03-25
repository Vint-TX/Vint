using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Validation.Events;

[ProtocolId(1455866538339)]
public class EmailInvalidEvent(
    string email
) : IEvent {
    public string Email => email;
}
