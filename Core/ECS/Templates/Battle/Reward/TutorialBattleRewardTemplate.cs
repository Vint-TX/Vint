using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Reward;

[ProtocolId(1514023810287)]
public class TutorialBattleRewardTemplate : BattleResultRewardTemplate {
    public IEntity Create() => Create("battle_rewards/tutorial");
}