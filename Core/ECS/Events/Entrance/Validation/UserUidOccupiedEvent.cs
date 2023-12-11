using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(1437991652726)]
public class UserUidOccupiedEvent(string username) : IEvent {
    [ProtocolName("uid")] public string Username => username;
}
