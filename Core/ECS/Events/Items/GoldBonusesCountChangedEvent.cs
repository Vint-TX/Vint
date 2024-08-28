using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Items;

[ProtocolId(1532516266008)]
public class GoldBonusesCountChangedEvent(
    long newCount
) : IEvent {
    public long NewCount { get; } = newCount;
}
