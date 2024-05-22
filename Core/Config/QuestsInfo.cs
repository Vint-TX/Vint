using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Utils;

namespace Vint.Core.Config;

public readonly record struct QuestsInfo(
    float ConditionChance,
    float RareChance,
    Dictionary<QuestType, QuestTypeInfo> Types,
    Dictionary<QuestRewardType, List<QuestRewardInfo>> Rewards,
    UpdatesInfo Updates
);

public readonly record struct QuestTypeInfo(
    int MinCommonValue,
    int MaxCommonValue,
    int MinRareValue,
    int MaxRareValue,
    int ConditionValue
);

public readonly record struct QuestRewardInfo(
    string TypeName,
    string EntityName,
    int MinAmount,
    int MaxAmount
) {
    public IEntity RewardEntity => GlobalEntities.GetEntity(TypeName, EntityName);

    public int GetAmount(int targetValue, int minValue, int maxValue) =>
        minValue == maxValue ? MaxAmount : MathUtils.Map(targetValue, minValue, maxValue, MinAmount, MaxAmount);
}

public readonly record struct UpdatesInfo(
    TimeSpan CompletedQuestDuration,
    List<TimeOnly> Times
) {
    public TimeOnly GetNextUpdate() {
        TimeOnly now = TimeOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        TimeOnly closestTime = Times.MinBy(time => Math.Abs((time - now).Ticks));

        return closestTime;
    }
}

public enum QuestRewardType {
    Common,
    Rare,
    Condition
}
