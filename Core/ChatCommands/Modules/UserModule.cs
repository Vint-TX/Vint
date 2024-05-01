using System.Reflection;
using System.Text;
using LinqToDB;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Database;
using Vint.Core.Database.Models;

namespace Vint.Core.ChatCommands.Modules;

[ChatCommandGroup("user", "Commands for all players", PlayerGroups.None)]
public class UserModule : ChatCommandModule {
    [ChatCommand("help", "Show list of commands or usage of specified command")]
    public async Task Help(
        ChatCommandContext ctx,
        [Option("command", "Name of command to get help", true)]
        string commandName = "") {
        if (!string.IsNullOrWhiteSpace(commandName)) {
            ChatCommand? command = ctx.ChatCommandProcessor.GetOrDefault(commandName);

            RequirePermissionsAttribute? requirePermissionsAttribute = command?.Method.GetCustomAttribute<RequirePermissionsAttribute>();
            ChatCommandGroupAttribute? chatCommandGroupAttribute = command?.ChatCommandGroupAttribute;

            if (command == null ||
                requirePermissionsAttribute != null &&
                (ctx.Connection.Player.Groups & requirePermissionsAttribute.Permissions) != requirePermissionsAttribute.Permissions ||
                chatCommandGroupAttribute != null &&
                (ctx.Connection.Player.Groups & chatCommandGroupAttribute.Permissions) != chatCommandGroupAttribute.Permissions) {
                await ctx.SendPrivateResponse($"Command '{commandName}' not found");
                return;
            }

            await ctx.SendPrivateResponse(command.ToString());
        } else {
            IEnumerable<string> commands = ctx.ChatCommandProcessor.GetAll()
                .Where(command => {
                    RequirePermissionsAttribute? requirePermissionsAttribute = command.Method.GetCustomAttribute<RequirePermissionsAttribute>();

                    if (requirePermissionsAttribute != null)
                        return (ctx.Connection.Player.Groups & requirePermissionsAttribute.Permissions) == requirePermissionsAttribute.Permissions;

                    ChatCommandGroupAttribute? chatCommandGroupAttribute = command.ChatCommandGroupAttribute;

                    if (chatCommandGroupAttribute != null)
                        return (ctx.Connection.Player.Groups & chatCommandGroupAttribute.Permissions) == chatCommandGroupAttribute.Permissions;

                    return true;
                })
                .Select(command => command.Info.ToString());

            string response = string.Join("\n", commands);
            await ctx.SendPrivateResponse(response);
        }
    }

    [ChatCommand("players", "Show online players count")]
    public async Task Players(ChatCommandContext ctx) =>
        await ctx.SendPrivateResponse($"{ctx.Connection.Server.PlayerConnections.Count} players online");

    [ChatCommand("battles", "Show current battles count")]
    public async Task Battles(ChatCommandContext ctx) =>
        await ctx.SendPrivateResponse($"{ctx.Connection.Server.BattleProcessor.BattlesCount} battles");

    [ChatCommand("ping", "Show ping")]
    public async Task Ping(ChatCommandContext ctx) =>
        await ctx.SendPrivateResponse($"Ping: {ctx.Connection.Ping}ms");

    [ChatCommand("stats", "Get statistics")]
    public async Task Stats(ChatCommandContext ctx) {
        await using DbConnection db = new();
        Statistics? statistics = await db.Statistics.SingleOrDefaultAsync(stats => stats.PlayerId == ctx.Connection.Player.Id);

        if (statistics == null) return;

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

        await ctx.SendPrivateResponse(builder.ToString());
    }
}
