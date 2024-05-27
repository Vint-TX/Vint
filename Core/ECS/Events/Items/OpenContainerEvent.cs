using LinqToDB;
using Vint.Core.Containers;
using Vint.Core.Database;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Notification;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;
using Container = Vint.Core.Database.Models.Container;

namespace Vint.Core.ECS.Events.Items;

[ProtocolId(1480325268669)]
public class OpenContainerEvent : IServerEvent {
    const int MaxAmount = 250;
    public long Amount { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        await using DbConnection db = new();

        IEntity userEntity = entities.Single();
        IEntity marketEntity = userEntity.GetMarketEntity(connection);
        Container? container = await db.Containers.SingleOrDefaultAsync(cont => cont.PlayerId == connection.Player.Id && cont.Id == marketEntity.Id);

        if (container == null || container.Count < Amount) return;

        Amount = Math.Clamp(container.Count, 1, MaxAmount); /*Math.Min(Amount, MaxAmount);*/

        container.Count -= Amount;
        await userEntity.ChangeComponent<UserItemCounterComponent>(component => component.Count = container.Count);
        await connection.Send(new ItemsCountChangedEvent(-Amount), userEntity);

        if (container.Count == 0) await db.DeleteAsync(container);
        else await db.UpdateAsync(container);

        List<IEntity> rewards = await ContainerRegistry.GetContainer(marketEntity)
            .Open(connection, Amount)
            .ToListAsync();

        await connection.Share(rewards);
        await connection.Send(new ShowNotificationGroupEvent(rewards.Count), marketEntity);
    }
}
