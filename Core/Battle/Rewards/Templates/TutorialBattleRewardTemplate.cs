using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Rewards.Templates;

[ProtocolId(1514023810287)]
public class TutorialBattleRewardTemplate : BattleResultRewardTemplate {
    public IEntity Create() => Create("battle_rewards/tutorial");
}
