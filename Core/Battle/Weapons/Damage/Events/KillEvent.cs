using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Damage.Events;

[ProtocolId(-8835994525014820133)]
public class KillEvent(
    IEntity killerMarketItem,
    IEntity target
) : IEvent {
    public IEntity KillerMarketItem { get; } = killerMarketItem;
    public IEntity Target { get; } = target;
}
