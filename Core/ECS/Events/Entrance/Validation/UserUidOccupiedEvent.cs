using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Entrance.Validation;

[ProtocolId(1437991652726)]
public class UserUidOccupiedEvent(
    string username
) : IEvent {
    [ProtocolName("Uid")] public string Username => username;
}