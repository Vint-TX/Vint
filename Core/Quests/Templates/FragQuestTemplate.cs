using Vint.Core.ECS.Entities;
using Vint.Core.Quests.Components.Type;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Quests.Templates;

[ProtocolId(1493731166999)]
public class FragQuestTemplate : QuestTemplate {
    public override IEntity Create(
        IEntity user,
        int index,
        float currentValue,
        float targetValue,
        IEntity reward,
        int rewardAmount,
        QuestConditionType? conditionType,
        long conditionValue,
        DateTimeOffset expireDate,
        QuestRarityType rarityType,
        bool isCompleted) {
        IEntity entity = base.Create(user,
            "quests/daily/frags",
            index,
            currentValue,
            targetValue,
            reward,
            rewardAmount,
            conditionType,
            conditionValue,
            expireDate,
            rarityType,
            isCompleted);

        entity.AddComponent<FragQuestComponent>();
        return entity;
    }
}
