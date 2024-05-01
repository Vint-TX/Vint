using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.Payment;

[ProtocolId(1473161186167)]
public class ExchangeCrystalsEvent : IServerEvent {
    public long XCrystals { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        int price = (int)XCrystals;
        await connection.PurchaseItem(GlobalEntities.GetEntity("misc", "Crystal"), price * 50, price, true, false);
    }
}
