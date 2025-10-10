using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Shop.Events;

[ProtocolId(1458203345903)]
public class BuyMarketItemEvent : IServerEvent {
    public int Price { get; private set; }
    public int Amount { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity marketItem = entities[1];

        if (!await GlobalEntities.ValidatePurchase(connection, marketItem, Amount, Price, false)) return;

        await connection.PurchaseItem(marketItem, Amount, Price, false, true);
    }
}
