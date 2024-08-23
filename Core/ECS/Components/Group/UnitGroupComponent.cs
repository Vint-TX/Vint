using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(1485231135123)]
public class UnitGroupComponent(
    long key
) : GroupComponent(key) {
    public UnitGroupComponent(IEntity entity) : this(entity.Id) { }
}
