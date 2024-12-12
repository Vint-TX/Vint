using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Enums;
using Vint.Core.ECS.Events.Notification;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.User.PromoCode;

[ProtocolId(1490877430206)]
public class ActivatePromoCodeEvent : IServerEvent {
    public string Code { get; private set; } = null!;

    public async Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider, IEnumerable<IEntity> entities) {
        PromoCodeCheckResult checkResult = await PromoCodeHelper.Check(connection.Player.Id, Code);

        if (checkResult != PromoCodeCheckResult.Valid)
            return;

        await using DbConnection db = new();
        await db.BeginTransactionAsync();

        var promo = await db.PromoCodes
            .Where(promoCode => promoCode.Code == Code)
            .LoadWith(promoCode => promoCode.Items)
            .Select(promoCode => new { promoCode.Id, promoCode.Items })
            .SingleAsync();

        await db.PromoCodes
            .Where(promoCode => promoCode.Id == promo.Id)
            .Set(promoCode => promoCode.Uses, promoCode => promoCode.Uses + 1)
            .UpdateAsync();

        PromoCodeRedemption redemption = new() {
            PlayerId = connection.Player.Id,
            PromoCodeId = promo.Id,
            RedeemedAt = DateTimeOffset.UtcNow
        };

        redemption.Id = await db.InsertWithInt64IdentityAsync(redemption);
        IEntity user = connection.UserContainer.Entity;

        await db.CommitTransactionAsync();

        foreach (PromoCodeItem item in promo.Items) {
            try {
                IEntity entity = GlobalEntities.AllMarketTemplateEntities.Single(entity => entity.Id == item.Id);

                if (await connection.CanOwnItem(entity))
                    await connection.PurchaseItem(entity, item.Quantity, 0, false, false);

                await connection.Share(new NewItemNotificationTemplate().CreateRegular(user, entity, item.Quantity));
            } catch (Exception e) {
                connection.Logger.Error(e, "An error occured while trying to redeem item {Id} from promo code {PromoId}", item.Id, promo.Id);
            }
        }

        await connection.Send(new ShowNotificationGroupEvent(), user);
    }
}
