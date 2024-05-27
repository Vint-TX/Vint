using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord;
using Vint.Core.ECS.Entities;
using Vint.Core.Protocol.Attributes;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.ECS.Events.Lobby;

[ProtocolId(1479879892222)]
public class ClientGarageFirstLoadEvent : IServerEvent {
    public async Task Execute(IPlayerConnection connection, IEnumerable<IEntity> entities) {
        DiscordBot? discordBot = connection.Server.DiscordBot;

        if (discordBot == null) return;

        Player player = connection.Player;

        if (player.DiscordLinked) {
            if (player.DiscordLink == null!) {
                player.DiscordLinked = false;
                player.DiscordUserId = default;

                await using (DbConnection db = new())
                    await db.Players
                        .Where(p => p.Id == player.Id)
                        .Set(p => p.DiscordLinked, false)
                        .Set(p => p.DiscordUserId, default(ulong))
                        .UpdateAsync();

                await connection.DisplayMessage("Your Discord account has been deauthorized");
            } else {
                (_, bool? isAuthorized) = await player.DiscordLink.GetClient(connection, discordBot);

                switch (isAuthorized) {
                    case false:
                        await connection.DisplayMessage("Your Discord account has been deauthorized");
                        break;

                    case null:
                        await connection.DisplayMessage("Cannot check Discord authorization. Something went wrong");
                        break;
                }
            }
        }

        if (!player.DiscordLinked)
            await connection.DisplayMessage(
                "Your Discord account is not linked! Secure your account by typing <color=orange><size=48>!link</size></color> in the chat",
                TimeSpanUtils.FromYears(40));
    }
}
