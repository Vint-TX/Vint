using System.Text;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using LinqToDB;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord.Utils;
using Vint.Core.Server;

namespace Vint.Core.Discord.Modules;

[Command("player")]
public class PlayerModule(
    GameServer gameServer
) {
    [Command("profile")]
    public async Task Profile(SlashCommandContext ctx, [Option("user", "Username in Vint")] string username) {
        await ctx.DeferResponseAsync();
        
        using DbConnection db = new();
        
        Player? targetPlayer = db.Players.SingleOrDefault(player => player.Username == username);
        

        if (targetPlayer == null) {
            DiscordEmbedBuilder error = Embeds.GetWarningEmbed($"player with username **{username}** does not exist.");
            await ctx.EditResponseAsync(error);
            return;
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"Status: Online/Offline/InBattle");
        builder.AppendLine($"");

        DiscordEmbedBuilder embed = Embeds.GetSuccessfulEmbed(builder.ToString(), $"{username} Profile");

        await ctx.EditResponseAsync(embed);
    }

    [Command("stats")]
    public async Task Stats(SlashCommandContext ctx, [Option("user", "Username in Vint")] string username) {
        await ctx.DeferResponseAsync();
        
        using DbConnection db = new();
        
        Player? targetPlayer = db.Players.LoadWith(player => player.Stats).SingleOrDefault(player => player.Username == username);

        if (targetPlayer == null) {
            DiscordEmbedBuilder error = Embeds.GetWarningEmbed($"Player {username} not found");
            await ctx.EditResponseAsync(error);
            return;
        };
        Statistics statistics = targetPlayer.Stats;

        StringBuilder builder = new();
        builder.AppendLine($"Kills: {statistics.Kills}");
        builder.AppendLine($"Deaths: {statistics.Deaths}");
        builder.AppendLine($"Victories: {statistics.Victories}");
        builder.AppendLine($"Defeats: {statistics.Defeats}");
        builder.AppendLine($"Crystals earned: {statistics.CrystalsEarned}");
        builder.AppendLine($"XCrystals earned: {statistics.XCrystalsEarned}");
        builder.AppendLine($"Shots: {statistics.Shots}");
        builder.AppendLine($"Hits: {statistics.Hits}");
        builder.AppendLine($"Flags delivered: {statistics.FlagsDelivered}");
        builder.AppendLine($"Flags returned: {statistics.FlagsReturned}");
        builder.AppendLine($"Gold boxes caught: {statistics.GoldBoxesCaught}");

        DiscordEmbedBuilder embed = Embeds.GetSuccessfulEmbed(builder.ToString(), $"{username} statistics");

        await ctx.EditResponseAsync(embed);
    }
}