using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Items;

[ProtocolId(1480931079801)]
public class ItemsCountChangedEvent(
    long delta
) : IEvent {
    public long Delta { get; private set; } = delta;
}