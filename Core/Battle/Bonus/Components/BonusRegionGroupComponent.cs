using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Bonus.Components;

[ProtocolId(8566120830355322079)]
public class BonusRegionGroupComponent(
    long key
) : GroupComponent(key) {
    public BonusRegionGroupComponent(IEntity entity) : this(entity.Id) { }
}
