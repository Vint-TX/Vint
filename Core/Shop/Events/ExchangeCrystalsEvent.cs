using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Payment;

[ProtocolId(1473161186167)]
public class ExchangeCrystalsEvent : IServerEvent {
    public long XCrystals { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        int price = (int)XCrystals;
        await connection.PurchaseItem(GlobalEntities.GetEntity("misc", "Crystal"), price * 50, price, true, false);
    }
}
