using Vint.Core.Battle.Rewards.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Rewards.Templates;

[ProtocolId(1514196284686)]
public class LevelUpUnlockBattleRewardTemplate : BattleResultRewardTemplate {
    public IEntity Create(List<IEntity> unlocked) {
        IEntity entity = Create("battle_rewards/lvlup_unlock");

        entity.AddComponent(new LevelUpUnlockPersonalRewardComponent(unlocked));
        return entity;
    }
}
