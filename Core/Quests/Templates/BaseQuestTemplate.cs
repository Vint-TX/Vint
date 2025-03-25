using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Quests.Components;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.User.Components;

namespace Vint.Core.Quests.Templates;

[ProtocolId(1474536285473)]
public abstract class BaseQuestTemplate : EntityTemplate {
    protected IEntity Create(
        IEntity user,
        string configPath,
        int index,
        float currentValue,
        float targetValue,
        IEntity reward,
        int rewardAmount,
        QuestConditionType? conditionType,
        long conditionValue,
        DateTimeOffset expireDate,
        QuestRarityType rarityType,
        bool isCompleted) => Entity(configPath,
        builder => builder
            .AddComponent<QuestComponent>()
            .AddComponent(new SlotIndexComponent(index))
            .AddComponent(new QuestProgressComponent(currentValue, targetValue))
            .AddComponent(new QuestRewardComponent(reward.Id, rewardAmount))
            .AddComponent(new QuestExpireDateComponent(expireDate))
            .AddComponent(new QuestRarityComponent(rarityType))
            .AddGroupComponent<UserGroupComponent>(user)
            .ThenExecuteIf(_ => conditionType != null,
                entity => entity.AddComponent(new QuestConditionComponent(conditionType!.Value, conditionValue)))
            .ThenExecuteIf(_ => isCompleted,
                entity => {
                    entity.AddComponent<CompleteQuestComponent>();
                    entity.AddComponent<RewardedQuestComponent>();
                }));
}
