using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Templates;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;

namespace Vint.Core.ECS.Events.User.PromoCode;

[ProtocolId(1490931976968)]
public class CheckPromoCodeEvent : IServerEvent { // todo
    public string Code { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        string[] parts = Code.Split('/');

        IEntity user = entities.Single();

        if (parts.Length != 2) {
            await connection.Send(new PromoCodeCheckResultEvent(Code, PromoCodeCheckResult.Invalid), user);
            return;
        }

        try {
            IEntity item = GlobalEntities.GetEntity(parts[0], parts[1]);

            if (item.TemplateAccessor?.Template is not MarketEntityTemplate ||
                item.GetUserEntity(connection).HasComponent<UserGroupComponent>()) {
                await connection.Send(new PromoCodeCheckResultEvent(Code, PromoCodeCheckResult.Invalid), user);
                return;
            }
        } catch {
            await connection.Send(new PromoCodeCheckResultEvent(Code, PromoCodeCheckResult.Invalid), user);
            return;
        }

        await connection.Send(new PromoCodeCheckResultEvent(Code, PromoCodeCheckResult.Valid), user);
    }
}
