using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Email.Events;

[ProtocolId(635906273457089964)]
public class EmailOccupiedEvent(
    string email
) : IEvent {
    public string Email => email;
}
