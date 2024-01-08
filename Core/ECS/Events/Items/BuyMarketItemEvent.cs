using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Items;

[ProtocolId(1458203345903)]
public class BuyMarketItemEvent : IServerEvent {
    public int Price { get; private set; }
    public int Amount { get; private set; }

    public void Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity marketItem = entities.ElementAt(1);

        if (!GlobalEntities.ValidatePurchase(connection, marketItem, Amount, Price, false)) return;

        connection.PurchaseItem(marketItem, Amount, Price, false, true);
    }
}