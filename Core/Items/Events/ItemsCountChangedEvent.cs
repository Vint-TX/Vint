using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Events;

[ProtocolId(1480931079801)]
public class ItemsCountChangedEvent(
    long delta
) : IEvent {
    public long Delta { get; private set; } = delta;
}
