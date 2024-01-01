using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(1485852459997)]
public class ModuleGroupComponent(
    long key
) : GroupComponent(key) {
    public ModuleGroupComponent(IEntity entity) : this(entity.Id) { }
}