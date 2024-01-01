using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(1544510801819)]
public class FractionGroupComponent(
    long key
) : GroupComponent(key) {
    public FractionGroupComponent(IEntity entity) : this(entity.Id) { }
}