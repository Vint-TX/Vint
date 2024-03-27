using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Vint.Core.Battles;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord.Utils;
using Vint.Core.Server;

namespace Vint.Core.Discord.Modules;

[SlashCommandGroup("player", "Player-specific commands")]
public class PlayerModule(
    GameServer gameServer
) : ApplicationCommandModule {
    [SlashCommand("profile", "Get player profile"), SlashCooldown(1, 60, SlashCooldownBucketType.User)]
    public async Task Profile(
        InteractionContext ctx,
        [Option("username", "Username in Vint")]
        string username) {
        await ctx.DeferAsync();

        IPlayerConnection? connection = gameServer.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);

        bool isOnline = connection != null;
        Player? targetPlayer = connection?.Player;

        if (targetPlayer == null) {
            await using DbConnection db = new();
            targetPlayer = db.Players.SingleOrDefault(player => player.Username == username);

            if (targetPlayer == null) {
                DiscordEmbedBuilder error = Embeds.GetErrorEmbed($"Player with username **{username}** does not exist.", critical: true);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(error));
                return;
            }
        }

        Battle? battle = connection?.BattlePlayer?.Battle;
        bool inBattle = battle != null;

        string status = isOnline
                            ? inBattle
                                  ? $"In battle: {battle!.MapInfo.Name}, {battle.Properties.BattleMode}"
                                  : "Online"
                            : "Offline";

        DiscordEmbedBuilder embed = Embeds.GetNotificationEmbed(status, $"{username}'s profile")
            .AddField("Experience", $"{targetPlayer.Experience}", true)
            .AddField("Reputation", $"{targetPlayer.Reputation}", true)
            .AddField("League", $"{targetPlayer.League}", true)
            .AddField("Registered", Formatter.Timestamp(targetPlayer.RegistrationTime, TimestampFormat.ShortDateTime), true)
            .AddField("Last login", Formatter.Timestamp(targetPlayer.LastLoginTime), true);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }

    [SlashCommand("statistics", "Get player statistics"), SlashCooldown(1, 60, SlashCooldownBucketType.User)]
    public async Task Statistics(
        InteractionContext ctx,
        [Option("user", "Username in Vint")] string username) {
        await ctx.DeferAsync();

        await using DbConnection db = new();

        long playerId = db.Players
            .Where(player => player.Username == username)
            .Select(player => player.Id)
            .SingleOrDefault();

        if (playerId == default) {
            DiscordEmbedBuilder error = Embeds.GetErrorEmbed($"Player with username **{username}** does not exist.", critical: true);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(error));
            return;
        }

        Statistics? statistics = db.Statistics.SingleOrDefault(stats => stats.PlayerId == playerId);

        if (statistics == null) {
            DiscordEmbedBuilder error = Embeds.GetErrorEmbed("Internal error. Report it to the Vint developers", critical: true);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(error));
            throw new UnreachableException($"Statistics for player '{username}' (id: {playerId}) does not exists");
        }

        DiscordEmbedBuilder embed = Embeds.GetNotificationEmbed("", $"{username}'s statistics")
            .AddField("Kills/Deaths", $"{statistics.Kills}/{statistics.Deaths} ({statistics.KD})", true)
            .AddField("Victories/Defeats", $"{statistics.Victories}/{statistics.Defeats} ({statistics.VD})", true)
            .AddField("Crystals earned", $"{statistics.CrystalsEarned}", true)
            .AddField("XCrystals earned", $"{statistics.XCrystalsEarned}", true)
            .AddField("Flags delivered", $"{statistics.FlagsDelivered}", true)
            .AddField("Flags returned", $"{statistics.FlagsReturned}", true)
            .AddField("GoldBoxes caught", $"{statistics.GoldBoxesCaught}", true);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}