using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Player.User.Events.Ping;

[ProtocolId(1480333153972)]
public class BattlePongEvent(
    float clientSendRealTime
) : IEvent {
    public float ClientSendRealTime { get; private set; } = clientSendRealTime;
}
