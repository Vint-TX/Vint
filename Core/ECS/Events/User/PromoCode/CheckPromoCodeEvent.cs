using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.User.PromoCode;

[ProtocolId(1490931976968)]
public class CheckPromoCodeEvent : IServerEvent {
    public string Code { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        PromoCodeCheckResult checkResult = await PromoCodeHelper.Check(connection.Player.Id, Code);
        await connection.Send(new PromoCodeCheckResultEvent(Code, checkResult), connection.UserContainer.Entity);
    }
}
