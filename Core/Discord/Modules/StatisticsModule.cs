using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Vint.Core.Battles;
using Vint.Core.Discord.Utils;
using Vint.Core.Server.Game;

namespace Vint.Core.Discord.Modules;

[Command("statistics")]
public class StatisticsModule(
    GameServer gameServer,
    IBattleProcessor battleProcessor
) {
    [Command("count")]
    public async Task Count(CommandContext ctx) {
        await ctx.DeferResponseAsync();

        int players = gameServer.PlayerConnections.Count;
        int battles = battleProcessor.Battles.Count;

        DiscordEmbedBuilder embed = Embeds
            .GetNotificationEmbed("")
            .AddField("Players", $"{players}", true)
            .AddField("Battles", $"{battles}", true);

        await ctx.EditResponseAsync(embed);
    }
}
