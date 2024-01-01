using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(63290793489633843)]
public class MarketItemGroupComponent(
    long key
) : GroupComponent(key) {
    public MarketItemGroupComponent(IEntity entity) : this(entity.Id) { }
}