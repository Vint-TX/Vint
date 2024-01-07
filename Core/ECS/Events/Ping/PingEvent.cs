using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Ping;

[ProtocolId(5356229304896471086)]
public class PingEvent(
    DateTimeOffset serverTime,
    sbyte commandId = 0 // ??
) : IEvent {
    public DateTimeOffset ServerTime { get; private set; } = serverTime;
    public sbyte CommandId { get; private set; } = commandId;
}