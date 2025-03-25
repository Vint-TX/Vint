using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Components;

[ProtocolId(63290793489633843)]
public class MarketItemGroupComponent(
    long key
) : GroupComponent(key) {
    public MarketItemGroupComponent(IEntity entity) : this(entity.Id) { }
}
