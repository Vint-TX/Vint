using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Entrance.Validation.Events;

[ProtocolId(1437991666522)]
public class UserUidVacantEvent(
    string username
) : IEvent {
    [ProtocolName("Uid")] public string Username => username;
}
