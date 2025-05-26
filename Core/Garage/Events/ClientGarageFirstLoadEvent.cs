using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Email;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Garage.Events;

[ProtocolId(1479879892222)]
public class ClientGarageFirstLoadEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.IsLoggedIn) return;

        Player player = connection.Player;

        if (player.EmailRewardsReceived || !player.EmailConfirmed)
            return;

        await EmailUtils.ReceiveEmailRewards(connection);

        await using DbConnection db = new();
        await db.Players.Where(p => p.Id == player.Id)
            .Set(p => p.EmailRewardsReceived, true)
            .UpdateAsync();

        player.EmailRewardsReceived = true;
    }
}
