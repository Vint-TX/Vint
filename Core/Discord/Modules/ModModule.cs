using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Vint.Core.Discord.Utils;
using Vint.Core.Server;

namespace Vint.Core.Discord.Modules;

[SlashCommandGroup("mod", "Commands for moderators", false), SlashRequireUserPermissions(Permissions.ModerateMembers)]
public class ModModule(
    GameServer gameServer
) : ApplicationCommandModule {
    [SlashCommand("dmsg", "Display message")]
    public async Task DisplayMessage(
        InteractionContext ctx,
        [Option("target", "Username of player or @a for broadcast")]
        string username,
        [Option("message", "Message to display")]
        string message) {
        await ctx.DeferAsync();

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
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(error));
                    return;
                }

                target.DisplayMessage(message);
                break;
            }
        }

        DiscordEmbedBuilder embed = Embeds.GetSuccessfulEmbed("Message displayed");
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }

    [SlashCommand("kick", "Kick the player")]
    public async Task KickPlayer(
        InteractionContext ctx,
        [Option("target", "Username of player")]
        string username,
        [Option("reason", "Reason for kick")] string? reason = null) {
        await ctx.DeferAsync();

        IPlayerConnection? targetConnection = gameServer.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);

        DiscordEmbedBuilder? error = null;

        if (targetConnection == null)
            error = Embeds.GetErrorEmbed("Player is offline");
        else if (targetConnection.Player.IsAdmin)
            error = Embeds.GetErrorEmbed($"Player '{username}' is admin");

        if (error != null) {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(error));
            return;
        }

        targetConnection?.Kick(reason);
        string punishMessage = $"{username} was kicked for '{reason}'";

        DiscordEmbedBuilder success = Embeds.GetSuccessfulEmbed(punishMessage);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(success));
    }
}