using Vint.Core.ECS.Components.Battle.Rewards;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Battle.Reward;

[ProtocolId(1514196284686)]
public class LevelUpUnlockBattleRewardTemplate : BattleResultRewardTemplate {
    public IEntity Create(List<IEntity> unlocked) {
        IEntity entity = Create("battle_rewards/lvlup_unlock");

        entity.AddComponent(new LevelUpUnlockPersonalRewardComponent(unlocked));
        return entity;
    }
}