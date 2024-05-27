using LinqToDB;
using LinqToDB.Linq;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Quest;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Quests;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Quest;

// og name: UseBonusEvent
[ProtocolId(1504703762311)]
public class RequestChangeQuestEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        if (!connection.IsOnline) return;

        entities = entities.ToList();

        QuestManager questManager = connection.Server.QuestManager;
        Player player = connection.Player;
        IEntity bonus = entities.ElementAt(0);
        IEntity quest = entities.ElementAt(1);

        await using DbConnection db = new();

        if (player.QuestChanges >= player.MaxQuestChanges) {
            player.QuestChangesResetTime = DateTimeOffset.UtcNow.AddDays(1);
            await bonus.AddComponentIfAbsent<TakenBonusComponent>();

            await db.Players
                .Where(p => p.Id == player.Id)
                .Set(p => p.QuestChangesResetTime, player.QuestChangesResetTime)
                .UpdateAsync();
            return;
        }

        await questManager.ChangeQuest(connection, quest);
        player.QuestChanges++;

        IUpdatable<Player> query = db.Players
            .Where(p => p.Id == player.Id)
            .Set(p => p.QuestChanges, player.QuestChanges);

        if (player.QuestChanges >= player.MaxQuestChanges) {
            player.QuestChangesResetTime = DateTimeOffset.UtcNow.AddDays(1);
            await bonus.AddComponentIfAbsent<TakenBonusComponent>();

            query = query.Set(p => p.QuestChangesResetTime, player.QuestChangesResetTime);
            connection.Schedule(player.QuestChangesResetTime.Value, async () => await questManager.ResetQuestChanges(connection));
        }

        await query.UpdateAsync();
    }
}
