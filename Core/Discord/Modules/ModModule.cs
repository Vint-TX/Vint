using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Vint.Core.Discord.Utils;
using Vint.Core.Server;

namespace Vint.Core.Discord.Modules;

[Command("mod"), RequirePermissions(DiscordPermissions.ModerateMembers)]
public class ModModule(
    GameServer gameServer
) {
    [Command("dmsg")]
    public async Task DisplayMessage(
        CommandContext ctx,
        string username,
        string message) {
        await ctx.DeferResponseAsync();

        switch (username) {
            case "@a": {
                foreach (IPlayerConnection connection in gameServer.PlayerConnections.Values)
                    connection.DisplayMessage(message);
                break;
            }

            default: {
                IPlayerConnection? target = gameServer.PlayerConnections.Values
                    .Where(conn => conn.IsOnline)
                    .SingleOrDefault(conn => conn.Player.Username == username);

                if (target == null) {
                    DiscordEmbedBuilder error = Embeds.GetErrorEmbed($"Player '{username}' not found");
                    await ctx.EditResponseAsync(error);
                    return;
                }

                target.DisplayMessage(message);
                break;
            }
        }

        DiscordEmbedBuilder embed = Embeds.GetSuccessfulEmbed("Message displayed");
        await ctx.EditResponseAsync(embed);
    }

    [Command("kick")]
    public async Task KickPlayer(
        CommandContext ctx,
        string username,
        string? reason = null) {
        await ctx.DeferResponseAsync();

        IPlayerConnection? targetConnection = gameServer.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);

        DiscordEmbedBuilder? error = null;

        if (targetConnection == null)
            error = Embeds.GetErrorEmbed("Player is offline");
        else if (targetConnection.Player.IsAdmin)
            error = Embeds.GetErrorEmbed($"Player '{username}' is admin");

        if (error != null) {
            await ctx.EditResponseAsync(error);
            return;
        }

        targetConnection?.Kick(reason);
        string punishMessage = $"{username} was kicked for '{reason}'";

        DiscordEmbedBuilder success = Embeds.GetSuccessfulEmbed(punishMessage);
        await ctx.EditResponseAsync(success);
    }
}
