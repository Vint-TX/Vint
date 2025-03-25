using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Unit.Components;

[ProtocolId(1485231135123)]
public class UnitGroupComponent(
    long key
) : GroupComponent(key) {
    public UnitGroupComponent(IEntity entity) : this(entity.Id) { }
}
