using LinqToDB;
using Vint.Core.DailyBonus.Components;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.DailyBonus.Events;

[ProtocolId(636461720141482519)]
public class SwitchDailyBonusCycleEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!DailyBonusUtils.CanSwitchCycle(connection))
            return;

        Player player = connection.Player;
        IEntity user = connection.UserContainer.Entity;

        await using (DbConnection db = new())
            await db.Players
                .Where(p => p.Id == player.Id)
                .Set(p => p.DailyBonusZone, 0)
                .Set(p => p.DailyBonusCycle, p => p.DailyBonusCycle + 1)
                .UpdateAsync();

        player.DailyBonusZone = 0;
        player.DailyBonusCycle++;

        await user.ChangeComponent<UserDailyBonusZoneComponent>(component => component.ZoneNumber = 0);
        await user.ChangeComponent<UserDailyBonusCycleComponent>(component => component.CycleNumber++);
        await user.ChangeComponent<UserDailyBonusReceivedRewardsComponent>(component => component.ReceivedRewards.Clear());
        await connection.Send<DailyBonusCycleSwitchedEvent>(user);
    }
}
