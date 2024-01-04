using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(1140613249019529884)]
public class BattleGroupComponent(
    long key
) : GroupComponent(key) {
    public BattleGroupComponent(IEntity entity) : this(entity.Id) { }
}