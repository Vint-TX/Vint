using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Vint.Core.Battle.Player;
using Vint.Core.Battle.Properties;
using Vint.Core.Battle.Results;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Tank;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.Logging;
using Vint.Core.Quests.Components;
using Vint.Core.Quests.Templates;
using Vint.Core.Server.Game;
using Vint.Core.Utils;

namespace Vint.Core.Quests;

public class QuestManager {
    const int MaxQuests = 4;

    public QuestManager(IServiceProvider serviceProvider) {
        ServiceProvider = serviceProvider;
        UpdateNextTime();
    }

    static QuestsInfo QuestsInfo => ConfigManager.QuestsInfo;
    ILogger Logger { get; } = Log.Logger.ForType<QuestManager>();

    IServiceProvider ServiceProvider { get; }
    DateTimeOffset NextUpdate { get; set; }

    public async Task<List<Quest>> SetupQuests(IPlayerConnection connection, bool deleteAllUncompleted) {
        List<Quest> quests = await GetCurrentQuests(connection.Player.Id, deleteAllUncompleted);

        List<IEntity> removedEntities = connection.SharedEntities
            .Where(entity => entity.HasComponent<QuestComponent>() && entity.HasComponent<SlotIndexComponent>())
            .Where(entity => quests.All(quest => quest.Index != entity.GetComponent<SlotIndexComponent>().Index))
            .ToList();

        foreach (IEntity removedEntity in removedEntities) {
            await connection.Unshare(removedEntity);
            removedEntity.Dispose();
        }

        await connection.Share(quests.Select(quest => GetQuestEntity(connection.UserContainer.Entity, quest)));

        while (quests.Count < MaxQuests) {
            int index = 0;

            while (quests.Any(quest => quest.Index == index))
                index++;

            bool canBeRare = quests.All(quest => quest.Rarity != QuestRarityType.Rare);
            bool canBeCondition = quests.All(quest => quest.Rarity != QuestRarityType.Condition);

            Quest quest = await CreateSaveAndShareQuest(connection, index, canBeRare, canBeCondition, false, quests.Select(quest => quest.Type));
            quests.Add(quest);
        }

        await TryCreatePremiumQuest(connection);

        Logger.Information("Created quests for {Username}", connection.Player.Username);
        return quests;
    }

    public async Task Tick(CancellationToken cancellationToken = default) {
        if (NextUpdate > DateTimeOffset.UtcNow)
            return;

        Logger.Warning("Updating quests...");

        ConfigManager.ServerConfig.LastQuestsUpdate = DateTimeOffset.UtcNow;
        await ConfigManager.ServerConfig.Save();

        await using DbConnection db = new();
        await db.BeginTransactionAsync(cancellationToken);

        GameServer server = ServiceProvider.GetRequiredService<GameServer>();

        foreach (IPlayerConnection connection in server.PlayerConnections.Values.Where(conn => conn.IsLoggedIn)) {
            try {
                await SetupQuests(connection, true);

                DateTimeOffset now = DateTimeOffset.UtcNow;

                await db.Players
                    .Where(player => player.Id == connection.Player.Id)
                    .Set(player => player.LastQuestUpdateTime, now)
                    .UpdateAsync(cancellationToken);

                connection.Player.LastQuestUpdateTime = now;
            } catch (OperationCanceledException) {
                // transaction will be rolled back automatically when the connection is disposed
                return;
            } catch (Exception e) {
                connection.Logger.Error(e, "Caught an error while updating the quests");
            }
        }

        await db.CommitTransactionAsync(cancellationToken);
        UpdateNextTime();

        Logger.Warning("Quests have been updated");
    }

    public async Task BattleFinished(Tanker tanker) {
        IPlayerConnection connection = tanker.Connection;
        Player player = connection.Player;
        Preset preset = player.CurrentPreset;
        BattleTank tank = tanker.Tank;
        Round round = tanker.Round;

        await using DbConnection db = new();
        List<Quest> quests = await db.Quests
            .Where(quest => quest.PlayerId == player.Id)
            .ToListAsync();

        foreach (Quest quest in quests.Where(quest => !quest.IsCompleted &&
                                                      quest.ConditionMet(preset.Weapon, preset.Hull, round.Properties.GetValue(BattleProperty.BattleMode)))) {
            IEntity? entity = connection.SharedEntities.SingleOrDefault(entity => entity.HasComponent<QuestComponent>() &&
                                                                                  entity.HasComponent<SlotIndexComponent>() &&
                                                                                  entity.GetComponent<SlotIndexComponent>().Index == quest.Index);

            if (entity == null) continue;

            int progressDelta = quest.Type switch {
                QuestType.Battles => 1,
                QuestType.Flags => tank.Result.Flags,
                QuestType.Frags => tank.Result.Kills,
                QuestType.Scores => tank.Tanker.GetScoreWithBonus(tank.Result.ScoreWithoutPremium),
                QuestType.Supply => tank.Result.BonusesTaken,
                QuestType.Victories => tank.Tanker.TeamResult == TeamBattleResult.Win ? 1 : 0,
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
        List<Quest> quests = await db.Quests
            .Where(quest => quest.PlayerId == connection.Player.Id)
            .ToListAsync();

        Quest? quest = quests.FirstOrDefault(quest => quest.Index == questEntity.GetComponent<SlotIndexComponent>().Index);

        if (quest == null) return;

        bool canBeRare = quests.All(q => q.Rarity != QuestRarityType.Rare);
        bool canBeCondition = quests.All(q => q.Rarity != QuestRarityType.Condition);

        await connection.Unshare(questEntity);
        await db.DeleteAsync(quest);

        await CreateSaveAndShareQuest(connection,
            quest.Index,
            canBeRare,
            canBeCondition,
            quest.Rarity == QuestRarityType.Premium,
            quests.Select(q => q.Type));
    }

    public async Task ResetQuestChanges(IPlayerConnection connection) {
        Player player = connection.Player;

        player.QuestChanges = 0;
        player.QuestChangesResetTime = null;

        await using (DbConnection db = new()) {
            await db.Players
                .Where(p => p.Id == player.Id)
                .Set(p => p.QuestChangesResetTime, player.QuestChangesResetTime)
                .Set(p => p.QuestChanges, player.QuestChanges)
                .UpdateAsync();
        }

        IEntity? bonus = connection.SharedEntities.FirstOrDefault(entity => entity.TemplateAccessor?.Template is QuestDailyBonusTemplate);

        if (bonus == null) return;

        await bonus.RemoveComponentIfPresent<TakenBonusComponent>();
    }

    public async Task TryCreatePremiumQuest(IPlayerConnection connection) {
        Player player = connection.Player;
        if (!player.HasPremiumQuest) return;

        DbConnection db = new();
        List<Quest> quests = await db.Quests
            .Where(quest => quest.PlayerId == player.Id)
            .ToListAsync();

        await db.DisposeAsync();

        if (quests.Any(quest => quest.Rarity == QuestRarityType.Premium))
            return;

        int index = 0;
        while (quests.Any(quest => quest.Index == index))
            index++;

        await CreateSaveAndShareQuest(connection, index, true, false, true, quests.Select(q => q.Type));
    }

    public async Task TryCleanupPremiumQuests(IPlayerConnection connection) {
        Player player = connection.Player;
        if (player.HasPremiumQuest) return;

        await using DbConnection db = new();
        List<Quest> quests = await db.Quests
            .Where(quest => quest.PlayerId == player.Id && quest.Rarity == QuestRarityType.Premium)
            .ToListAsync();

        if (quests.Count == 0) return;

        List<IEntity> questEntities = connection.SharedEntities
            .Where(entity => entity.HasComponent<QuestComponent>() &&
                             entity.HasComponent<SlotIndexComponent>())
            .ToList();

        await db.BeginTransactionAsync();

        foreach (Quest quest in quests) {
            IEntity? entity = questEntities.FirstOrDefault(entity => entity.GetComponent<SlotIndexComponent>().Index == quest.Index);

            if (entity != null) {
                await connection.Unshare(entity);
                entity.Dispose();
            }

            await db.DeleteAsync(quest);
        }

        await db.CommitTransactionAsync();
    }

    async Task QuestCompleted(IPlayerConnection connection, Quest quest, IEntity entity) {
        quest.CompletionDate = DateTimeOffset.UtcNow;
        DateTimeOffset updateTime = quest.CompletionDate.Value + QuestsInfo.Updates.CompletedQuestUpdateDurations.GetDuration(quest.Rarity);

        await connection.PurchaseItem(quest.RewardEntity, quest.RewardAmount, 0, false, false);

        await entity.ChangeComponent<QuestProgressComponent>(component => component.CurrentComplete = true);
        await entity.ChangeComponent<QuestExpireDateComponent>(component => component.Date = updateTime);
        connection.Schedule(updateTime, async () => await ChangeQuest(connection, entity));
    }

    void UpdateNextTime() {
        NextUpdate = DateTimeOffset.UtcNow + QuestsInfo.Updates.GetDurationToNextUpdate();
        Logger.Information("Quests will be updated on {Time}", NextUpdate);
    }

    static async Task<Quest> CreateSaveAndShareQuest(
        IPlayerConnection connection,
        int index,
        bool canBeRare,
        bool canBeCondition,
        bool isPremium,
        IEnumerable<QuestType> excludeTypes) {
        Quest quest = isPremium
            ? GeneratePremiumQuest(connection.Player, index, excludeTypes)
            : GenerateQuest(connection.Player, index, canBeRare, canBeCondition, excludeTypes);

        IEntity questEntity = GetQuestEntity(connection.UserContainer.Entity, quest);

        await using (DbConnection db = new())
            await db.InsertAsync(quest);

        await connection.Share(questEntity);
        return quest;
    }

    static async Task<List<Quest>> GetCurrentQuests(long playerId, bool deleteAllUncompleted) {
        await using DbConnection db = new();
        List<Quest> quests = await db.Quests
            .Where(quest => quest.PlayerId == playerId)
            .ToListAsync();

        DateTimeOffset now = DateTimeOffset.UtcNow;
        List<Quest> questsToDelete = quests.Where(quest => ShouldDelete(quest, deleteAllUncompleted, now)).ToList();

        if (questsToDelete.Count == 0)
            return quests;

        await db.BeginTransactionAsync();

        foreach (Quest quest in questsToDelete) {
            await db.DeleteAsync(quest);
            quests.Remove(quest);
        }

        await db.CommitTransactionAsync();
        return quests;

        static bool ShouldDelete(Quest quest, bool deleteAllUncompleted, DateTimeOffset currentTime) {
            if (quest is { Rarity: QuestRarityType.Premium, IsCompleted: false })
                return false;

            if (deleteAllUncompleted && !quest.IsCompleted)
                return true;

            if (!quest.IsCompleted)
                return false;

            TimeSpan updateDuration = QuestsInfo.Updates.CompletedQuestUpdateDurations.GetDuration(quest.Rarity);
            DateTimeOffset updateTime = quest.CompletionDate.Value + updateDuration;
            return updateTime <= currentTime;
        }
    }

    static IEntity GetQuestEntity(IEntity user, Quest quest) {
        TimeSpan updateDuration = QuestsInfo.Updates.CompletedQuestUpdateDurations.GetDuration(quest.Rarity);
        DateTimeOffset updateTime = (quest.CompletionDate + updateDuration) ?? DateTimeOffset.UtcNow;

        return GetQuestTemplate(quest.Type).Create(user,
            quest.Index,
            quest.ProgressCurrent,
            quest.ProgressTarget,
            quest.RewardEntity,
            quest.RewardAmount,
            quest.Condition,
            quest.ConditionValue,
            updateTime,
            quest.Rarity,
            quest.IsCompleted);
    }

    static QuestTemplate GetQuestTemplate(QuestType type) => type switch {
        QuestType.Battles => new BattleCountQuestTemplate(),
        QuestType.Flags => new FlagQuestTemplate(),
        QuestType.Frags => new FragQuestTemplate(),
        QuestType.Scores => new ScoreQuestTemplate(),
        QuestType.Supply => new SupplyQuestTemplate(),
        QuestType.Victories => new WinQuestTemplate(),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    static Quest GenerateQuest(Player player, int index, bool canBeRare, bool canBeCondition, IEnumerable<QuestType> excludeTypes) {
        bool withCondition = canBeCondition && MathUtils.RollTheDice(QuestsInfo.ConditionChance);
        bool isRare = !withCondition && canBeRare && MathUtils.RollTheDice(QuestsInfo.RareChance);

        (QuestType questType, QuestTypeInfo questInfo) = GetRandomQuestInfo(excludeTypes);

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

    static Quest GeneratePremiumQuest(Player player, int index, IEnumerable<QuestType> excludeTypes) {
        (QuestType questType, QuestTypeInfo questInfo) = GetRandomQuestInfo(excludeTypes);

        Range valuesRange = GetValuesRange(questInfo, true, false);
        int targetValue = Random.Shared.Next(valuesRange.Start.Value, valuesRange.End.Value + 1);

        QuestRewardInfo rewardInfo = GetRandomReward(QuestRewardType.Premium);

        return new Quest {
            Player = player,
            Index = index,
            Type = questType,
            ProgressTarget = targetValue,
            RewardEntity = rewardInfo.RewardEntity,
            RewardAmount = rewardInfo.GetAmount(targetValue, valuesRange.Start.Value, valuesRange.End.Value),
            Rarity = QuestRarityType.Premium
        };
    }

    static KeyValuePair<QuestType, QuestTypeInfo> GetRandomQuestInfo(IEnumerable<QuestType> excludeTypes) {
        List<KeyValuePair<QuestType, QuestTypeInfo>> types = QuestsInfo.Types.Where(info => !excludeTypes.Contains(info.Key)).ToList();

        return types.Count != 0
            ? types.RandomElement()
            : QuestsInfo.Types.ToList().RandomElement();
    }

    static (QuestConditionType?, long) GenerateCondition(bool withCondition) {
        if (!withCondition) return (null, 0);

        QuestConditionType conditionType = Enum
            .GetValues<QuestConditionType>()
            .Where(type => type != QuestConditionType.Mode)
            .ToList()
            .RandomElement();

        long value;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (conditionType) {
            case QuestConditionType.Weapon:
                IEntity weapon = GlobalEntities
                    .GetEntities("weapons")
                    .ToList()
                    .RandomElement();

                value = weapon.Id;
                break;

            case QuestConditionType.Tank:
                IEntity hull = GlobalEntities
                    .GetEntities("hulls")
                    .ToList()
                    .RandomElement();

                value = hull.Id;
                break;

            default:
                throw new IndexOutOfRangeException();
        }

        return (conditionType, value);
    }

    static Range GetValuesRange(QuestTypeInfo questInfo, bool isRare, bool withCondition) =>
        withCondition ? questInfo.ConditionValue..questInfo.ConditionValue :
        isRare ? questInfo.MinRareValue..questInfo.MaxRareValue : questInfo.MinCommonValue..questInfo.MaxCommonValue;

    static QuestRewardType GetRewardType(bool isRare, bool withCondition) =>
        withCondition ? QuestRewardType.Condition : isRare ? QuestRewardType.Rare : QuestRewardType.Common;

    static QuestRarityType GetRarityType(bool isRare, bool withCondition) =>
        withCondition ? QuestRarityType.Condition : isRare ? QuestRarityType.Rare : QuestRarityType.Common;

    static QuestRewardInfo GetRandomReward(QuestRewardType rewardType) =>
        QuestsInfo.Rewards[rewardType].RandomElement();
}
