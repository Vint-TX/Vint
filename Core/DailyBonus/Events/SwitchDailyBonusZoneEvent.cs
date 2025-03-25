using LinqToDB;
using Vint.Core.DailyBonus.Components;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.DailyBonus.Events;

[ProtocolId(636461688330473034)]
public class SwitchDailyBonusZoneEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!DailyBonusUtils.CanSwitchZone(connection))
            return;

        Player player = connection.Player;
        IEntity user = connection.UserContainer.Entity;

        await using (DbConnection db = new()) {
            await db.Players
                .Where(p => p.Id == player.Id)
                .Set(p => p.DailyBonusZone, p => p.DailyBonusZone + 1)
                .UpdateAsync();
        }

        player.DailyBonusZone++;

        await user.ChangeComponent<UserDailyBonusZoneComponent>(component => component.ZoneNumber++);
        await connection.Send<DailyBonusZoneSwitchedEvent>(user);
    }
}
