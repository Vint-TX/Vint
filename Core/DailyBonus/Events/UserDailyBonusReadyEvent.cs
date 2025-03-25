using LinqToDB;
using Vint.Core.DailyBonus.Components;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.DailyBonus.Events;

[ProtocolId(1497606008075)]
public class UserDailyBonusReadyEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.IsLoggedIn)
            return;

        Player player = connection.Player;
        IEntity user = connection.UserContainer.Entity;

        if (!user.HasComponent<DailyBonusReadyComponent>())
            return;

        await Cleanup(user);

        DateTimeOffset nextDailyBonusTime = player.NextDailyBonusTime.Value;
        DateTimeOffset lastDailyBonusTime = player.LastDailyBonusReceivingTime;

        await user.AddComponent(new UserDailyBonusCycleComponent(player.DailyBonusCycle));
        await user.AddComponent(new UserDailyBonusZoneComponent(player.DailyBonusZone));
        await user.AddComponent(new UserDailyBonusNextReceivingDateComponent(nextDailyBonusTime, lastDailyBonusTime));

        await using (DbConnection db = new()) {
            List<int> receivedRewards = await db.DailyBonusRedemptions
                .Where(redemption => redemption.PlayerId == player.Id &&
                                     redemption.Cycle == player.DailyBonusCycle)
                .Select(redemption => redemption.Code)
                .ToListAsync();

            await user.AddComponent(new UserDailyBonusReceivedRewardsComponent(receivedRewards) { OwnerUserId = user.Id } );
        }

        await user.AddComponent<UserDailyBonusInitializedComponent>();
    }

    static async Task Cleanup(IEntity user) {
        await user.RemoveComponentIfPresent<UserDailyBonusCycleComponent>();
        await user.RemoveComponentIfPresent<UserDailyBonusZoneComponent>();
        await user.RemoveComponentIfPresent<UserDailyBonusNextReceivingDateComponent>();
        await user.RemoveComponentIfPresent<UserDailyBonusReceivedRewardsComponent>();
        await user.RemoveComponentIfPresent<UserDailyBonusInitializedComponent>();
    }
}
