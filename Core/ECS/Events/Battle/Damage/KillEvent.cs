using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Damage;

[ProtocolId(-8835994525014820133)]
public class KillEvent(
    IEntity killerMarketItem,
    IEntity target
) : IEvent {
    public IEntity KillerMarketItem { get; } = killerMarketItem;
    public IEntity Target { get; } = target;
}