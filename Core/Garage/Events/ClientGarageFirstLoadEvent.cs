using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Events;
using Vint.Core.Email;
using Vint.Core.Server.Game.Connection;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Garage.Events;

[ProtocolId(1479879892222)]
public class ClientGarageFirstLoadEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEntity[] entities) {
        if (!connection.IsLoggedIn) return;

        await CheckEmailRewards(connection);
        await CheckPremiumRewards(connection);
    }

    static async Task CheckEmailRewards(IPlayerConnection connection) {
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

    static async Task CheckPremiumRewards(IPlayerConnection connection) =>
        await connection.CheckPremiumBoostBonuses();
}
