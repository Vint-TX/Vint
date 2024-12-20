using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Items;

[ProtocolId(1473424321578)]
public class XBuyMarketItemEvent : IServerEvent {
    public int Price { get; private set; }
    public int Amount { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        IEntity marketItem = entities[1];

        if (!await GlobalEntities.ValidatePurchase(connection, marketItem, Amount, Price, true)) return;

        await connection.PurchaseItem(marketItem, Amount, Price, true, true);
    }
}
