using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Items;

[ProtocolId(1473424321578)]
public class XBuyMarketItemEvent : IServerEvent {
    public int Price { get; private set; }
    public int Amount { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        IEntity marketItem = entities.ElementAt(1);

        if (!await GlobalEntities.ValidatePurchase(connection, marketItem, Amount, Price, true)) return;

        await connection.PurchaseItem(marketItem, Amount, Price, true, true);
    }
}
