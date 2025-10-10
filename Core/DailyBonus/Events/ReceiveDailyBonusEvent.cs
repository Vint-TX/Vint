using LinqToDB;
using Vint.Core.DailyBonus.Components;
using Vint.Core.DailyBonus.Components.Config.Cycles;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.DailyBonus.Events;

[ProtocolId(636458159324341964)]
public class ReceiveDailyBonusEvent : IServerEvent {
    public int Code { get; private set; }

    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (!DailyBonusUtils.TeleportCharged(connection) || DailyBonusUtils.RewardReceived(connection, Code))
            return;

        Player player = connection.Player;
        IEntity user = connection.UserContainer.Entity;

        int cycle = player.DailyBonusCycle;
        int zone = player.DailyBonusZone;

        DailyBonusCycleComponent cycleConfig = DailyBonusUtils.GetCycleConfig(cycle);
        int maxCode = cycleConfig.Zones[zone] + 1;

        if (Code > maxCode)
            return;

        DailyBonusData? bonus = cycleConfig.DailyBonuses.FirstOrDefault(bonus => bonus.Code == Code);

        if (bonus == null)
            return;

        switch (bonus.DailyBonusType) {
            case DailyBonusType.Cry: {
                long crystals = bonus.CryAmount!.Value;
                await connection.ChangeCrystals(crystals);
                break;
            }

            case DailyBonusType.XCry: {
                long xCrystals = bonus.XCryAmount!.Value;
                await connection.ChangeXCrystals(xCrystals);
                break;
            }

            case DailyBonusType.Container: {
                DailyBonusGarageItemReward containerReward = bonus.ContainerReward!;
                IEntity marketItem = connection.GetEntity(containerReward.MarketItemId)!;

                await connection.PurchaseItem(marketItem, containerReward.Amount, 0, false, false);
                break;
            }

            case DailyBonusType.Detail: {
                DailyBonusGarageItemReward detailReward = bonus.DetailReward!;
                IEntity marketItem = connection.GetEntity(detailReward.MarketItemId)!;

                await connection.PurchaseItem(marketItem, detailReward.Amount, 0, false, false);
                break;
            }

            case DailyBonusType.None:
                throw new InvalidOperationException("Daily bonus type is not set");

            case DailyBonusType.Energy:
                throw new NotSupportedException("Energy bonus is not supported");

            default:
                throw new ArgumentOutOfRangeException(null);
        }

        DailyBonusRedemption redemption = new() {
            Code = Code,
            PlayerId = player.Id,
            RedeemedAt = now,
            Zone = zone,
            Cycle = cycle
        };

        await using (DbConnection db = new()) {
            await db.BeginTransactionAsync();

            redemption.Id = await db.InsertWithInt64IdentityAsync(redemption);
            await db.Players
                .Where(p => p.Id == player.Id)
                .Set(p => p.LastDailyBonusReceivingTime, now)
                .UpdateAsync();

            await db.CommitTransactionAsync();
        }

        player.LastDailyBonusReceivingTime = now;
        player.ResetNextDailyBonusTime();

        await user.ChangeComponent<UserDailyBonusReceivedRewardsComponent>(component => component.ReceivedRewards.Add(Code));
        await user.ChangeComponent(new UserDailyBonusNextReceivingDateComponent(player.NextDailyBonusTime.Value, now));

        await connection.Send(new DailyBonusReceivedEvent(Code), user);
    }
}
