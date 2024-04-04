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
    public long Amount { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        using DbConnection db = new();

        IEntity userEntity = entities.Single();
        IEntity marketEntity = userEntity.GetMarketEntity(connection);
        Container? container = db.Containers.SingleOrDefault(cont => cont.PlayerId == connection.Player.Id && cont.Id == marketEntity.Id);

        if (container == null || container.Count < Amount) return;

        Amount = Math.Min(Amount, 100);

        container.Count -= Amount;
        userEntity.ChangeComponent<UserItemCounterComponent>(component => component.Count = container.Count);
        connection.Send(new ItemsCountChangedEvent(-Amount), userEntity);

        if (container.Count == 0) db.Delete(container);
        else db.Update(container);

        List<IEntity> rewards = ContainerRegistry.GetContainer(marketEntity).Open(connection, Amount).ToList();

        connection.Share(rewards);
        connection.Send(new ShowNotificationGroupEvent(rewards.Count), marketEntity);
    }
}