using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Events;

[ProtocolId(1532516266008)]
public class GoldBonusesCountChangedEvent(
    long newCount
) : IEvent {
    public long NewCount { get; } = newCount;
}
