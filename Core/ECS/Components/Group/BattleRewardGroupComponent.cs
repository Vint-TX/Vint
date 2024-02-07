using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Group;

[ProtocolId(1513677547945)]
public class BattleRewardGroupComponent(
    long key
) : GroupComponent(key) {
    public BattleRewardGroupComponent(IEntity entity) : this(entity.Id) { }
}