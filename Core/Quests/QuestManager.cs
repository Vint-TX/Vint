using LinqToDB;
using Serilog;
using Vint.Core.Battles;
using Vint.Core.Battles.Player;
using Vint.Core.Battles.Type;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Quest;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates.Quests;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Quests;

public class QuestManager(
    GameServer gameServer
) {
    const int MaxQuests = 4;
    static QuestsInfo QuestsInfo => ConfigManager.QuestsInfo;
    static ILogger Logger { get; } = Log.Logger.ForType(typeof(QuestManager));

    public async Task<List<Quest>> SetupQuests(IPlayerConnection connection, bool deleteAllUncompleted) {
        List<Quest> quests = await GetCurrentQuests(connection.Player.Id, deleteAllUncompleted);
        List<IEntity> removedEntities = connection.SharedEntities
            .Where(entity => entity.HasComponent<QuestComponent>() &&
                             entity.HasComponent<SlotIndexComponent>())
            .DistinctBy(entity => quests.Any(quest => quest.Index == entity.GetComponent<SlotIndexComponent>().Index))
            .ToList();

        await connection.Unshare(removedEntities);
        await connection.Share(quests.Select(quest => GetQuestEntity(connection.User, quest)));

        while (quests.Count < MaxQuests) {
            int index = 0;

            while (quests.Any(quest => quest.Index == index))
                index++;

            bool canBeRare = quests.All(quest => quest.Rarity != QuestRarityType.Rare);
            bool canBeCondition = quests.All(quest => quest.Rarity != QuestRarityType.Condition);

            Quest quest = await CreateSaveAndShareQuest(connection, index, canBeRare, canBeCondition, quests.Select(quest => quest.Type));
            quests.Add(quest);
        }

        Logger.Information("Created quests for {Username}", connection.Player.Username);
        return quests;
    }

    // ReSharper disable once FunctionNeverReturns
    public async Task Loop() {
        while (true) {
            TimeOnly currentTime = TimeOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
            TimeOnly nextTime = QuestsInfo.Updates.GetNextUpdate();
            TimeSpan duration = (nextTime - currentTime).Duration();

            Logger.Warning("Quests will be updated in {Duration}", duration);

            await Task.Delay(duration);

            Logger.Warning("Updating quests...");

            ConfigManager.ServerConfig.LastQuestsUpdate = DateTimeOffset.UtcNow;
            await ConfigManager.ServerConfig.Save();

            await using DbConnection db = new();
            await db.BeginTransactionAsync();

            foreach (IPlayerConnection connection in gameServer.PlayerConnections.Values.Where(conn => conn.IsOnline)) {
                try {
                    await SetupQuests(connection, true);

                    DateTimeOffset now = DateTimeOffset.UtcNow;

                    await db.Players
                        .Where(player => player.Id == connection.Player.Id)
                        .Set(player => player.LastQuestUpdateTime, now)
                        .UpdateAsync();

                    connection.Player.LastQuestUpdateTime = now;
                } catch (Exception e) {
                    connection.Logger.Error(e, "Caught an error while updating the quests");
                }
            }

            await db.CommitTransactionAsync();
            Logger.Warning("Quests have been updated");
        }
    }

    public async Task BattleFinished(IPlayerConnection connection, bool hasEnemies) {
        if (!hasEnemies ||
            !connection.InLobby ||
            !connection.BattlePlayer!.InBattleAsTank ||
            connection.BattlePlayer.Battle.TypeHandler is not MatchmakingHandler) return;

        BattleTank tank = connection.BattlePlayer.Tank!;
        Battle battle = tank.Battle;
        Preset preset = connection.Player.CurrentPreset;

        await using DbConnection db = new();

        List<Quest> quests = await db.Quests
            .Where(quest => quest.PlayerId == connection.Player.Id)
            .ToListAsync();

        foreach (Quest quest in quests.Where(quest => !quest.IsCompleted &&
                                                      quest.ConditionMet(preset.Weapon, preset.Hull, battle.Properties.BattleMode))) {
            IEntity? entity = connection.SharedEntities.SingleOrDefault(entity => entity.HasComponent<QuestComponent>() &&
                                                                                  entity.HasComponent<SlotIndexComponent>() &&
                                                                                  entity.GetComponent<SlotIndexComponent>().Index == quest.Index);

            if (entity == null) continue;

            int progressDelta = quest.Type switch {
                QuestType.Battles => 1,
                QuestType.Flags => tank.Result.Flags,
                QuestType.Frags => tank.Result.Kills,
                QuestType.Scores => tank.Result.Score,
                QuestType.Supply => tank.Result.BonusesTaken,
                QuestType.Victories => tank.BattlePlayer.TeamBattleResult == TeamBattleResult.Win ? 1 : 0,
                _ => 0
            };

            if (progressDelta == 0) continue;

            quest.AddProgress(progressDelta);
            await entity.ChangeComponent<QuestProgressComponent>(component => component.CurrentValue = quest.ProgressCurrent);

            if (quest.IsCompleted)
                await QuestCompleted(connection, quest, entity);

            await db.UpdateAsync(quest);
        }
    }

    public async Task ChangeQuest(IPlayerConnection connection, IEntity questEntity) {
        await using DbConnection db = new();

        List<Quest> quests = await db.Quests.Where(quest => quest.PlayerId == connection.Player.Id).ToListAsync();
        Quest? quest = quests.SingleOrDefault(quest => quest.Index == questEntity.GetComponent<SlotIndexComponent>().Index);

        if (quest == null) return;

        bool canBeRare = quests.All(q => q.Rarity != QuestRarityType.Rare);
        bool canBeCondition = quests.All(q => q.Rarity != QuestRarityType.Condition);

        await connection.Unshare(questEntity);
        await db.DeleteAsync(quest);
        await CreateSaveAndShareQuest(connection, quest.Index, canBeRare, canBeCondition, quests.Select(q => q.Type));
    }

    public async Task ResetQuestChanges(IPlayerConnection connection) {
        Player player = connection.Player;

        player.QuestChanges = 0;
        player.QuestChangesResetTime = null;

        await using DbConnection db = new();
        await db.Players
            .Where(p => p.Id == player.Id)
            .Set(p => p.QuestChangesResetTime, player.QuestChangesResetTime)
            .Set(p => p.QuestChanges, player.QuestChanges)
            .UpdateAsync();

        IEntity? bonus = connection.SharedEntities.SingleOrDefault(entity => entity.TemplateAccessor?.Template is QuestDailyBonusTemplate);

        if (bonus == null) return;

        await bonus.RemoveComponentIfPresent<TakenBonusComponent>();
    }

    async Task QuestCompleted(IPlayerConnection connection, Quest quest, IEntity entity) {
        quest.CompletionDate = DateTimeOffset.UtcNow;

        await connection.PurchaseItem(quest.RewardEntity, quest.RewardAmount, 0, false, false);

        await using DbConnection db = new();
        await db.UpdateAsync(quest);

        await entity.ChangeComponent<QuestProgressComponent>(component => component.CurrentComplete = true);
        await entity.ChangeComponent<QuestExpireDateComponent>(component => component.Date = quest.CompletedQuestChangeTime!.Value);
        connection.Schedule(quest.CompletedQuestChangeTime!.Value, async () => await ChangeQuest(connection, entity));
    }

    static async Task<Quest> CreateSaveAndShareQuest(
        IPlayerConnection connection,
        int index,
        bool canBeRare,
        bool canBeCondition,
        IEnumerable<QuestType> usedTypes) {
        Quest quest = GenerateQuest(connection.Player, index, canBeRare, canBeCondition, usedTypes);
        IEntity questEntity = GetQuestEntity(connection.User, quest);

        await using DbConnection db = new();
        await db.InsertAsync(quest);

        await connection.Share(questEntity);
        return quest;
    }

    static async Task<List<Quest>> GetCurrentQuests(long playerId, bool deleteAllUncompleted) {
        await using DbConnection db = new();
        List<Quest> quests = await db.Quests.Where(quest => quest.PlayerId == playerId).ToListAsync();

        foreach (Quest quest in quests
                     .ToList()
                     .Where(quest =>
                         deleteAllUncompleted && !quest.IsCompleted ||
                         quest.IsCompleted && DateTimeOffset.UtcNow - quest.CompletionDate >= QuestsInfo.Updates.CompletedQuestDuration)) {
            await db.DeleteAsync(quest);
            quests.Remove(quest);
        }

        return quests;
    }

    static IEntity GetQuestEntity(IEntity user, Quest quest) =>
        GetQuestTemplate(quest.Type)
            .Create(user,
                quest.Index,
                quest.ProgressCurrent,
                quest.ProgressTarget,
                quest.RewardEntity,
                quest.RewardAmount,
                quest.Condition,
                quest.ConditionValue,
                quest.CompletedQuestChangeTime ?? DateTimeOffset.UtcNow,
                quest.Rarity,
                quest.IsCompleted);

    static QuestTemplate GetQuestTemplate(QuestType type) =>
        type switch {
            QuestType.Battles => new BattleCountQuestTemplate(),
            QuestType.Flags => new FlagQuestTemplate(),
            QuestType.Frags => new FragQuestTemplate(),
            QuestType.Scores => new ScoreQuestTemplate(),
            QuestType.Supply => new SupplyQuestTemplate(),
            QuestType.Victories => new WinQuestTemplate(),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

    static Quest GenerateQuest(Player player, int index, bool canBeRare, bool canBeCondition, IEnumerable<QuestType> usedTypes) {
        bool withCondition = canBeCondition && MathUtils.RollTheDice(QuestsInfo.ConditionChance);
        bool isRare = !withCondition && canBeRare && MathUtils.RollTheDice(QuestsInfo.RareChance);

        (QuestType questType, QuestTypeInfo questInfo) = GetRandomQuestInfo(usedTypes);

        Range valuesRange = GetValuesRange(questInfo, isRare, withCondition);
        int targetValue = Random.Shared.Next(valuesRange.Start.Value, valuesRange.End.Value + 1);

        (QuestConditionType? conditionType, long conditionValue) = GenerateCondition(withCondition);
        QuestRewardInfo rewardInfo = GetRandomReward(GetRewardType(isRare, withCondition));

        return new Quest {
            Player = player,
            Index = index,
            Type = questType,
            ProgressTarget = targetValue,
            RewardEntity = rewardInfo.RewardEntity,
            RewardAmount = rewardInfo.GetAmount(targetValue, valuesRange.Start.Value, valuesRange.End.Value),
            Rarity = GetRarityType(isRare, withCondition),
            Condition = conditionType,
            ConditionValue = conditionValue
        };
    }

    static KeyValuePair<QuestType, QuestTypeInfo> GetRandomQuestInfo(IEnumerable<QuestType> usedTypes) =>
        QuestsInfo.Types
            .Where(info => !usedTypes.Contains(info.Key))
            .ToList()
            .Shuffle()
            .First();

    static (QuestConditionType?, long) GenerateCondition(bool withCondition) {
        if (!withCondition) return (null, default);

        QuestConditionType conditionType = Enum.GetValues<QuestConditionType>()
            .Where(type => type != QuestConditionType.Mode)
            .ToList()
            .Shuffle()
            .First();

        long value;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (conditionType) {
            case QuestConditionType.Weapon:
                IEntity weapon = GlobalEntities.GetEntities("weapons").ToList().Shuffle().First();
                value = weapon.Id;
                break;

            case QuestConditionType.Tank:
                IEntity hull = GlobalEntities.GetEntities("hulls").ToList().Shuffle().First();
                value = hull.Id;
                break;

            default:
                throw new IndexOutOfRangeException();
        }

        return (conditionType, value);
    }

    static Range GetValuesRange(QuestTypeInfo questInfo, bool isRare, bool withCondition) =>
        withCondition
            ? questInfo.ConditionValue..questInfo.ConditionValue
            : isRare
                ? questInfo.MinRareValue..questInfo.MaxRareValue
                : questInfo.MinCommonValue..questInfo.MaxCommonValue;

    static QuestRewardType GetRewardType(bool isRare, bool withCondition) =>
        withCondition
            ? QuestRewardType.Condition
            : isRare
                ? QuestRewardType.Rare
                : QuestRewardType.Common;

    static QuestRarityType GetRarityType(bool isRare, bool withCondition) =>
        withCondition
            ? QuestRarityType.Condition
            : isRare
                ? QuestRarityType.Rare
                : QuestRarityType.Common;

    static QuestRewardInfo GetRandomReward(QuestRewardType rewardType) =>
        QuestsInfo.Rewards[rewardType].Shuffle().First();
}
