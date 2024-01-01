using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(635906273457089964)]
public class EmailOccupiedEvent(
    string email
) : IEvent {
    public string Email => email;
}