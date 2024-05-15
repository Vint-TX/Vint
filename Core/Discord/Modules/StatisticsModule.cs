using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Vint.Core.Discord.Utils;
using Vint.Core.Server;

namespace Vint.Core.Discord.Modules;

[Command("statistics")]
public class StatisticsModule(
    GameServer gameServer
) {
    [Command("count")]
    public async Task Count(CommandContext ctx) {
        await ctx.DeferResponseAsync();

        int players = gameServer.PlayerConnections.Count;
        int battles = gameServer.BattleProcessor.BattlesCount;

        DiscordEmbedBuilder embed = Embeds.GetNotificationEmbed("")
            .AddField("Players", $"{players}", true)
            .AddField("Battles", $"{battles}", true);

        await ctx.EditResponseAsync(embed);
    }
}
