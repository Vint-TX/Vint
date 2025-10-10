using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.PromoCodes.Events;

[ProtocolId(1490931976968)]
public class CheckPromoCodeEvent : IServerEvent {
    public string Code { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        PromoCodeCheckResult checkResult = await PromoCodeHelper.Check(connection.Player.Id, Code);
        await connection.Send(new PromoCodeCheckResultEvent(Code, checkResult), connection.UserContainer.Entity);
    }
}
