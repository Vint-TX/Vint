using Vint.Core.ECS.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Rewards.Components;

[ProtocolId(1513677547945)]
public class BattleRewardGroupComponent(
    long key
) : GroupComponent(key) {
    public BattleRewardGroupComponent(IEntity entity) : this(entity.Id) { }
}
