using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Vint.Core.Battle.Lobby;
using Vint.Core.Discord.Utils;
using Vint.Core.Server.Game;

namespace Vint.Core.Discord.Modules;

[Command("statistics")]
public class StatisticsModule(
    GameServer gameServer,
    LobbyProcessor lobbyProcessor
) {
    [Command("count")]
    public async Task Count(CommandContext ctx) {
        await ctx.DeferResponseAsync();

        int players = gameServer.PlayerConnections.Count;
        int lobbies = lobbyProcessor.Count;

        DiscordEmbedBuilder embed = Embeds
            .GetNotificationEmbed("")
            .AddField("Players", $"{players}", true)
            .AddField("Battles", $"{lobbies}", true);

        await ctx.EditResponseAsync(embed);
    }
}
