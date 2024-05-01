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

    public Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        string[] parts = Code.Split('/');

        IEntity user = entities.Single();

        if (parts.Length != 2) {
            connection.Send(new PromoCodeCheckResultEvent(Code, PromoCodeCheckResult.Invalid), user);
            return Task.CompletedTask;
        }

        try {
            IEntity item = GlobalEntities.GetEntity(parts[0], parts[1]);

            if (item.TemplateAccessor?.Template is not MarketEntityTemplate ||
                item.GetUserEntity(connection).HasComponent<UserGroupComponent>()) {
                connection.Send(new PromoCodeCheckResultEvent(Code, PromoCodeCheckResult.Invalid), user);
                return Task.CompletedTask;
            }
        } catch {
            connection.Send(new PromoCodeCheckResultEvent(Code, PromoCodeCheckResult.Invalid), user);
            return Task.CompletedTask;
        }

        connection.Send(new PromoCodeCheckResultEvent(Code, PromoCodeCheckResult.Valid), user);
        return Task.CompletedTask;
    }
}
