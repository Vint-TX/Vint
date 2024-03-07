using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(8566120830355322079)]
public class BonusRegionGroupComponent(
    long key
) : GroupComponent(key) {
    public BonusRegionGroupComponent(IEntity entity) : this(entity.Id) { }
}