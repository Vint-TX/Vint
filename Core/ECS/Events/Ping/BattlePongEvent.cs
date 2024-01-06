using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Ping;

[ProtocolId(1480333153972)]
public class BattlePongEvent(
    float clientSendRealTime
) : IEvent {
    public float ClientSendRealTime { get; private set; } = clientSendRealTime;
}