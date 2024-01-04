using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(4088029591333632383)]
public class TankGroupComponent(
    long key
) : GroupComponent(key) {
    public TankGroupComponent(IEntity entity) : this(entity.Id) { }
}