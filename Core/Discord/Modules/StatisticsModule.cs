using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using Vint.Core.Discord.Utils;
using Vint.Core.Server;

namespace Vint.Core.Discord.Modules;

[Command("statistics")]
public class StatisticsModule(
    GameServer gameServer
) {
    [Command("players")]
    public async Task Players(SlashCommandContext ctx) {
        await ctx.DeferResponseAsync();

        int count = gameServer.PlayerConnections.Count;
        DiscordEmbedBuilder embed = Embeds.GetNotificationEmbed($"{count} players online");

        await ctx.EditResponseAsync(embed);
    }

    [Command("battles")]
    public async Task Battles(SlashCommandContext ctx) {
        await ctx.DeferResponseAsync();

        int count = gameServer.BattleProcessor.BattlesCount;
        DiscordEmbedBuilder embed = Embeds.GetNotificationEmbed($"{count} battles");

        await ctx.EditResponseAsync(embed);
    }
}