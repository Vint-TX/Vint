using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Fractions.Components;

[ProtocolId(1544510801819)]
public class FractionGroupComponent(
    long key
) : GroupComponent(key) {
    public FractionGroupComponent(IEntity entity) : this(entity.Id) { }
}
