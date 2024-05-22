using LinqToDB.Mapping;
using Vint.Core.Battles;
using Vint.Core.Config;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;

namespace Vint.Core.Database.Models;

[Table("Quests")]
public class Quest {
    [NotColumn] readonly Player _player = null!;

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))]
    public required Player Player {
        get => _player;
        init {
            _player = value;
            PlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PlayerId { get; private set; }

    [PrimaryKey(1)] public required int Index { get; init; }
    [Column] public required QuestType Type { get; init; }

    [Column] public int ProgressCurrent { get; private set; }
    [Column] public required int ProgressTarget { get; init; }

    [Column] public required IEntity RewardEntity { get; init; }
    [Column] public required int RewardAmount { get; init; }

    [Column] public required QuestRarityType Rarity { get; init; }
    [Column] public DateTimeOffset? CompletionDate { get; set; }

    [Column] public QuestConditionType? Condition { get; init; }
    [Column] public long ConditionValue { get; init; }

    [NotColumn] public bool IsCompleted => ProgressCurrent == ProgressTarget;
    [NotColumn] public DateTimeOffset? CompletedQuestChangeTime => CompletionDate + ConfigManager.QuestsInfo.Updates.CompletedQuestDuration;

    public void AddProgress(int delta) =>
        ProgressCurrent = Math.Clamp(ProgressCurrent + delta, 0, ProgressTarget);

    public bool ConditionMet(IEntity marketWeapon, IEntity marketHull, BattleMode battleMode) =>
        Condition switch {
            null => true,
            QuestConditionType.Weapon => ConditionValue == marketWeapon.Id,
            QuestConditionType.Tank => ConditionValue == marketHull.Id,
            QuestConditionType.Mode => (BattleMode)ConditionValue == battleMode,
            _ => throw new InvalidOperationException()
        };
}

public enum QuestType {
    Battles,
    Flags,
    Frags,
    Scores,
    Supply,
    Victories
}
