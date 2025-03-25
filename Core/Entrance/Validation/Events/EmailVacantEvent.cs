using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(635906273700499964)]
public class EmailVacantEvent(
    string email
) : IEvent {
    public string Email => email;
}
