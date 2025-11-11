using LinqToDB;
using LinqToDB.Async;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Items.Components;
using Vint.Core.Items.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.DailyBonus.Events;

[ProtocolId(636464215401773226)]
public class ReceiveTargetItemFromDetailsByDailyBonusEvent : IServerEvent {
    public long DetailMarketItemId { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!DailyBonusUtils.DailyBonusesUnlocked(connection))
            return;

        Player player = connection.Player;
        IEntity user = connection.UserContainer.Entity;

        IEntity marketEntity = connection.GetEntity(DetailMarketItemId)!;
        DetailItemComponent detailItemComponent = ConfigManager.GetComponent<DetailItemComponent>(marketEntity.TemplateAccessor!.ConfigPath!);
        int detailsCount;

        await using (DbConnection db = new()) {
            IQueryable<Detail> query = db.Details.Where(detail => detail.PlayerId == player.Id && detail.Id == DetailMarketItemId);

            detailsCount = await query.Select(detail => detail.Count).FirstOrDefaultAsync();
            if (detailsCount == 0 || detailsCount < detailItemComponent.RequiredCount) return;

            detailsCount -= detailItemComponent.RequiredCount;

            if (detailsCount == 0) await query.DeleteAsync();
            else await query.Set(detail => detail.Count, detailsCount).UpdateAsync();
        }

        IEntity userEntity = marketEntity.GetUserEntity(connection);
        await userEntity.ChangeComponent<UserItemCounterComponent>(component => component.Count = detailsCount);
        await connection.Send(new ItemsCountChangedEvent(-detailItemComponent.RequiredCount), userEntity);

        IEntity targetMarketEntity = connection.GetEntity(detailItemComponent.TargetMarketItemId)!;
        await connection.PurchaseItem(targetMarketEntity, 1, 0, false, false);

        await connection.Send(new TargetItemFromDailyBonusReceivedEvent(DetailMarketItemId), user);
    }
}
