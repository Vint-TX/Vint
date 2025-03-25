using LinqToDB;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Components.Item;
using Vint.Core.ECS.Components.Server.Shop;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events.Items;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.DailyBonus;

[ProtocolId(636464215401773226)]
public class ReceiveTargetItemFromDetailsByDailyBonusEvent : IServerEvent {
    public long DetailMarketItemId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!DailyBonusUtils.DailyBonusesUnlocked(connection))
            return;

        Player player = connection.Player;
        IEntity user = connection.UserContainer.Entity;

        await using DbConnection db = new();
        Detail? detail = await db.Details.SingleOrDefaultAsync(detail => detail.PlayerId == player.Id && detail.Id == DetailMarketItemId);

        if (detail == null)
            return;

        IEntity marketEntity = connection.GetEntity(DetailMarketItemId)!;
        IEntity userEntity = marketEntity.GetUserEntity(connection);
        DetailItemComponent detailItemComponent = ConfigManager.GetComponent<DetailItemComponent>(marketEntity.TemplateAccessor!.ConfigPath!);

        if (detail.Count < detailItemComponent.RequiredCount)
            return;

        detail.Count -= detailItemComponent.RequiredCount;
        await userEntity.ChangeComponent<UserItemCounterComponent>(component => component.Count = detail.Count);
        await connection.Send(new ItemsCountChangedEvent(-detailItemComponent.RequiredCount), userEntity);

        if (detail.Count == 0) await db.DeleteAsync(detail);
        else await db.UpdateAsync(detail);

        IEntity targetMarketEntity = connection.GetEntity(detailItemComponent.TargetMarketItemId)!;
        await connection.PurchaseItem(targetMarketEntity, 1, 0, false, false);

        await connection.Send(new TargetItemFromDailyBonusReceivedEvent(DetailMarketItemId), user);
    }
}
