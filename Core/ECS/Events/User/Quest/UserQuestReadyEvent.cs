using LinqToDB;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Quest;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates.Quests;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Quests;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.Quest;

[ProtocolId(1497606008074)]
public class UserQuestReadyEvent : IServerEvent { // todo premium
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.IsOnline) return;

        Player player = connection.Player;
        QuestManager questManager = connection.Server.QuestManager;

        bool updateQuests = player.LastQuestUpdateTime < ConfigManager.ServerConfig.LastQuestsUpdate;
        bool noChanges = player.QuestChanges >= player.MaxQuestChanges;

        List<Database.Models.Quest> quests = await questManager.SetupQuests(connection, updateQuests);

        if (updateQuests) {
            await using DbConnection db = new();
            await db.Players
                .Where(p => p.Id == player.Id)
                .Set(p => p.LastQuestUpdateTime, DateTimeOffset.UtcNow)
                .UpdateAsync();
        }

        if (noChanges && player.QuestChangesResetTime != null) {
            if (player.QuestChangesResetTime <= DateTimeOffset.UtcNow) {
                await questManager.ResetQuestChanges(connection);
                noChanges = false;
            } else {
                connection.Schedule(player.QuestChangesResetTime.Value, async () => await questManager.ResetQuestChanges(connection));
            }
        }

        List<IEntity> questEntities = connection.SharedEntities
            .Where(entity => entity.HasComponent<QuestComponent>() &&
                             entity.HasComponent<SlotIndexComponent>())
            .ToList();

        foreach (Database.Models.Quest quest in quests.Where(quest => quest.IsCompleted)) {
            IEntity entity = questEntities.First(entity => entity.GetComponent<SlotIndexComponent>().Index == quest.Index);
            connection.Schedule(quest.CompletedQuestChangeTime!.Value, async () => await questManager.ChangeQuest(connection, entity));
        }

        await connection.Share(new QuestDailyBonusTemplate().Create(connection.User, noChanges));
    }
}
