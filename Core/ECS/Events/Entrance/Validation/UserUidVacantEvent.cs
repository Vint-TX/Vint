using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(1437991666522)]
public class UserUidVacantEvent(string username) : IEvent {
    [ProtocolName("uid")] public string Username => username;
}