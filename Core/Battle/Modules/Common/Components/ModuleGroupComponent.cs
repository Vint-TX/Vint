using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Components;

[ProtocolId(1485852459997)]
public class ModuleGroupComponent(
    long key
) : GroupComponent(key) {
    public ModuleGroupComponent(IEntity entity) : this(entity.Id) { }
}
