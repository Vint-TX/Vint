using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using LinqToDB;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord.Utils;
using Vint.Core.Server;

namespace Vint.Core.Discord.Modules;

[Command("admin"), RequirePermissions(DiscordPermissions.Administrator)]
public class AdminModule(
    GameServer gameServer
) {
    [Command("ban")]
    public async Task BanPlayer(
        CommandContext ctx,
        string username,
        TimeSpan? duration = null,
        string? reason = null) {
        await ctx.DeferResponseAsync();

        IPlayerConnection? targetConnection = gameServer.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);

        Player? targetPlayer = targetConnection?.Player;

        if (targetConnection == null) {
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Username == username);
        }

        DiscordEmbedBuilder? error = null;

        if (targetPlayer == null)
            error = Embeds.GetErrorEmbed("Player not found");
        else if (targetPlayer.IsAdmin)
            error = Embeds.GetErrorEmbed($"Player '{username}' is admin");

        if (error != null) {
            await ctx.EditResponseAsync(error);
            return;
        }

        Punishment punishment = await targetPlayer!.Ban((targetConnection as SocketPlayerConnection)?.EndPoint.Address.ToString(), reason, duration);
        targetConnection?.Kick(reason);

        string description = $"Moderator: {ctx.User.Mention}\n";

        if (reason != null)
            description += $"Reason: {reason}\n";

        if (punishment.EndTime != null) {
            string timestamp = Formatter.Timestamp(punishment.EndTime.Value, TimestampFormat.LongDateTime);
            description += $"Until: {timestamp}";
        } else
            description += "Banned permanently";

        await ctx.EditResponseAsync(Embeds.GetSuccessfulEmbed(description, $"{username} was banned", $"Punishment Id: {punishment.Id}"));
    }

    [Command("unban")]
    public async Task UnbanPlayer(CommandContext ctx, string username) {
        await ctx.DeferResponseAsync();

        IPlayerConnection? targetConnection = gameServer.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);

        Player? targetPlayer = targetConnection?.Player;

        if (targetConnection == null) {
            await using DbConnection db = new();
            targetPlayer = await db.Players.SingleOrDefaultAsync(player => player.Username == username);
        }

        if (targetPlayer == null) {
            await ctx.EditResponseAsync(Embeds.GetErrorEmbed("Player not found"));
            return;
        }

        await targetPlayer.UnBan();

        string description = $"Moderator: {ctx.User.Mention}\n";

        await ctx.EditResponseAsync(Embeds.GetSuccessfulEmbed(description, $"{username} was unbanned"));
    }

    [Command("createInvite")]
    public async Task CreateInvite(CommandContext ctx, string code, ushort uses) {
        await ctx.DeferResponseAsync();

        await using DbConnection db = new();
        Invite? invite = await db.Invites.SingleOrDefaultAsync(invite => invite.Code == code);

        if (invite != null) {
            await ctx.EditResponseAsync(Embeds.GetErrorEmbed($"Already exists: {invite}"));
            return;
        }

        invite = new Invite {
            Code = code,
            RemainingUses = uses
        };

        invite.Id = await db.InsertWithInt64IdentityAsync(invite);
        await ctx.EditResponseAsync(Embeds.GetSuccessfulEmbed($"{invite}"));
    }
}
