using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using Vint.Core.Battles.Player;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Discord.Utils;
using Vint.Core.Server;

namespace Vint.Core.Discord.Modules;

[Command("mod")]
public class ModModule(
    GameServer gameServer
) {
    [Command("dmsg")]
    public async Task DisplayMessage(SlashCommandContext ctx, [Option("target")] string username, [Option("text")] string message) {
        await ctx.DeferResponseAsync();
        
        //TODO(Kurays): Add role check
        
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
}