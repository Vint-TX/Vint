using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LinqToDB;
using Vint.Core.Battle.Lobby;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord.Utils;
using Vint.Core.Server.Game;

namespace Vint.Core.Discord.Modules;

// todo make a cooldown

[Command("player")]
public class PlayerModule(
    GameServer gameServer
) {
    [Command("profile")]
    public async Task Profile(CommandContext ctx) {
        await ctx.DeferResponseAsync();

        DbConnection db = new();
        var player = await db.Players
            .Select(player => new {
                player.Id,
                player.DiscordUserId,
                player.Username,
                player.Experience,
                player.Reputation,
                player.League,
                player.RegistrationTime,
                player.LastLoginTime
            })
            .FirstOrDefaultAsync(player => player.DiscordUserId == ctx.User.Id);

        await db.DisposeAsync();

        if (player == null) {
            DiscordEmbedBuilder error = Embeds.GetErrorEmbed("You have not linked your Discord account with the game", critical: true);
            await ctx.EditResponseAsync(error);
            return;
        }

        IPlayerConnection? connection = gameServer.FindConnection(player.Id);
        bool isOnline = connection != null;

        LobbyBase? lobby = connection?.LobbyPlayer?.Lobby;
        bool inBattle = lobby != null;

        string status = isOnline
            ? inBattle
                ? $"In battle: {lobby!.Properties.MapInfo.Name}, {lobby.Properties.BattleMode}"
                : "Online"
            : "Offline";

        DiscordEmbedBuilder embed = Embeds
            .GetNotificationEmbed(status, $"{player.Username} profile")
            .AddField("Experience", $"{player.Experience}", true)
            .AddField("Reputation", $"{player.Reputation}", true)
            .AddField("League", $"{player.League}", true)
            .AddField("Registered", Formatter.Timestamp(player.RegistrationTime, TimestampFormat.ShortDateTime), true)
            .AddField("Last login", Formatter.Timestamp(player.LastLoginTime), true);

        await ctx.EditResponseAsync(embed);
    }

    [Command("statistics")]
    public async Task Statistics(CommandContext ctx) {
        await ctx.DeferResponseAsync();

        DbConnection db = new();
        var player = await db.Players
            .LoadWith(player => player.Stats)
            .Where(player => player.DiscordUserId == ctx.User.Id)
            .Select(player => new { player.Stats, player.Username })
            .FirstOrDefaultAsync();

        await db.DisposeAsync();

        if (player == null) {
            DiscordEmbedBuilder error = Embeds.GetErrorEmbed("You have not linked your Discord account with the game", critical: true);
            await ctx.EditResponseAsync(error);
            return;
        }

        Statistics statistics = player.Stats;

        DiscordEmbedBuilder embed = Embeds
            .GetNotificationEmbed("", $"{player.Username} statistics")
            .AddField("Kills/Deaths", $"{statistics.Kills}/{statistics.Deaths} ({statistics.KD})", true)
            .AddField("Victories/Defeats", $"{statistics.Victories}/{statistics.Defeats} ({statistics.VD})", true)
            .AddField("Crystals earned", $"{statistics.CrystalsEarned}", true)
            .AddField("XCrystals earned", $"{statistics.XCrystalsEarned}", true)
            .AddField("Flags delivered", $"{statistics.FlagsDelivered}", true)
            .AddField("Flags returned", $"{statistics.FlagsReturned}", true)
            .AddField("GoldBoxes caught", $"{statistics.GoldBoxesCaught}", true);

        await ctx.EditResponseAsync(embed);
    }

    [Command("restorePasswordCode")]
    public async Task RestorePasswordCode(SlashCommandContext ctx) {
        await ctx.DeferResponseAsync(true);

        IPlayerConnection? connection =
            gameServer.PlayerConnections.Values.SingleOrDefault(conn => conn.RestorePasswordCode != null && conn.Player.DiscordUserId == ctx.User.Id);

        if (connection == null) return;

        await ctx.EditResponseAsync(Embeds.GetWarningEmbed($"Your confirmation code: {Formatter.Spoiler(connection.RestorePasswordCode!)}",
            "Do not share this code with anyone!",
            "If someone asks you to send them this code, contact the administrators immediately!"));
    }
}
