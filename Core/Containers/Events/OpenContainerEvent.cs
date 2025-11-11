using LinqToDB;
using LinqToDB.Async;
using Vint.Core.Database;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Items.Components;
using Vint.Core.Items.Events;
using Vint.Core.Notification.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.Containers.Events;

[ProtocolId(1480325268669)]
public class OpenContainerEvent : IServerEvent {
    const int MaxAmount = 250;
    public long Amount { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (Amount <= 0) return;

        IEntity userEntity = entities.Single();
        IEntity marketEntity = userEntity.GetMarketEntity(connection);

        long containersCount;

        await using (DbConnection db = new()) {
            IQueryable<Database.Models.Container> query = db.Containers.Where(cont => cont.PlayerId == connection.Player.Id &&
                                                                                      cont.Id == marketEntity.Id);

            containersCount = await query.Select(cont => cont.Count).FirstOrDefaultAsync();
            if (containersCount < Amount) return;

            Amount = Math.Clamp(containersCount, 1, MaxAmount);
            containersCount -= Amount;

            if (containersCount == 0) await query.DeleteAsync();
            else await query.Set(cont => cont.Count, containersCount).UpdateAsync();
        }

        await userEntity.ChangeComponent<UserItemCounterComponent>(component => component.Count = containersCount);
        await connection.Send(new ItemsCountChangedEvent(-Amount), userEntity);

        List<IEntity> rewards = await ContainerRegistry
            .GetContainer(marketEntity)
            .Open(connection, Amount)
            .ToListAsync();

        await connection.Share(rewards);
        await connection.Send<ShowNotificationGroupEvent>(marketEntity);
    }
}
