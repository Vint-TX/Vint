using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using Vint.Core.Battles;
using Vint.Core.Battles.Player;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Discord.Utils;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint.Core.Discord.Modules;

[Command("mod")]
public class ModModule(
    GameServer gameServer
) {
    [Command("dmsg")]
    public async Task DisplayMessage(SlashCommandContext ctx, [Option("target")] string username, [Option("text")] string message) {
        await ctx.DeferResponseAsync();
        
        if (!ctx.Member.Permissions.HasPermission(Permissions.KickMembers)) {
            DiscordEmbedBuilder error = Embeds.GetErrorEmbed($"No permissions!");
            await ctx.EditResponseAsync(error);
            return;
        }
        
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
                    DiscordEmbedBuilder error = Embeds.GetWarningEmbed($"Player '{username}' not found");
                    await ctx.EditResponseAsync(error);
                    return;
                }

                target.DisplayMessage(message);
                break;
            }
        }
        DiscordEmbedBuilder embed = Embeds.GetSuccessfulEmbed($"Done");

        await ctx.EditResponseAsync(embed);
    }

    [Command("kick")]
    public async Task KickPlayer(SlashCommandContext ctx, [Option("target")] string username, [Option("reason")] string reason) {
        await ctx.DeferResponseAsync();
        
        if (!ctx.Member.Permissions.HasPermission(Permissions.KickMembers)) {
            DiscordEmbedBuilder error = Embeds.GetErrorEmbed($"No permissions!");
            await ctx.EditResponseAsync(error);
            return;
        }
        
        IPlayerConnection? targetConnection = gameServer.PlayerConnections.Values
            .Where(conn => conn.IsOnline)
            .SingleOrDefault(conn => conn.Player.Username == username);
        
        IEntity? notifyChat = null;
        List<IPlayerConnection>? notifiedConnections = null;

        if (targetConnection == null) {
            DiscordEmbedBuilder noTarget = Embeds.GetWarningEmbed($"Player is not on the server");
            await ctx.EditResponseAsync(noTarget);
            return;
        }

        if (targetConnection.Player.IsAdmin) {
            DiscordEmbedBuilder targetAdmin = Embeds.GetWarningEmbed($"Player '{username}' is admin");
            await ctx.EditResponseAsync(targetAdmin);
            return;
        }
        

        targetConnection.Kick(reason);
        string punishMessage = $"{username} was kicked for '{reason}'";
        DiscordEmbedBuilder success = Embeds.GetSuccessfulEmbed(punishMessage);
        await ctx.EditResponseAsync(success);
    }
}