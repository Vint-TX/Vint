using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Ping;

[ProtocolId(3963540336787160114)]
public class PingResultEvent(
    DateTimeOffset serverTime,
    float ping
) : IEvent {
    public DateTimeOffset ServerTime { get; set; } = serverTime;
    public float Ping { get; set; } = ping;
}