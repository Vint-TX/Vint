using System.Reflection;
using System.Text;
using LinqToDB;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord;

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

    [ChatCommand("link", "Link your account with Discord"), RequireConditions(ChatCommandConditions.InGarage)]
    public async Task Link(ChatCommandContext ctx) {
        DiscordBot? discordBot = ctx.GameServer.DiscordBot;

        if (discordBot == null) {
            await ctx.SendPrivateResponse("Cannot request account linking without Discord bot");
            return;
        }

        await ctx.SendPrivateResponse("Checking link status...");

        if (ctx.Connection.Player.DiscordLinked) {
            (_, bool? isAuthorized) = await ctx.Connection.Player.DiscordLink.GetClient(ctx.Connection, discordBot);

            switch (isAuthorized) {
                case true:
                    await ctx.SendPrivateResponse($"Your account is already linked. Discord user id: {ctx.Connection.Player.DiscordUserId}");
                    return;

                case null:
                    await ctx.SendPrivateResponse("Cannot check Discord authorization. Something went wrong");
                    return;
            }
        }

        long userId = ctx.Connection.User.Id;
        DiscordLinkRequest linkRequest = ConfigManager.DiscordLinkRequests.SingleOrDefault(req => req.UserId == userId);

        if (linkRequest == default) {
            byte[] stateBytes = new byte[32];
            Random.Shared.NextBytes(stateBytes);
            string state = Convert.ToHexString(stateBytes);

            linkRequest = new DiscordLinkRequest(state, userId);
            ConfigManager.DiscordLinkRequests.Add(linkRequest);
        }

        await OpenURL(linkRequest);
        return;

        async Task OpenURL(DiscordLinkRequest req) {
            DiscordConfig config = ConfigManager.Discord;
            Uri uri = discordBot.GetOAuth2Uri(config.OAuth2Redirect, req.State, config.OAuth2Scopes);

            await ctx.SendPrivateResponse("Authorization page will be opened in your browser soon");
            await ctx.Connection.OpenURL(uri.ToString());
        }
    }

    [ChatCommand("unlink", "(Not recommended) Unlink your account with Discord"), RequireConditions(ChatCommandConditions.InGarage)]
    public async Task Unlink(ChatCommandContext ctx) {
        DiscordBot? discordBot = ctx.GameServer.DiscordBot;

        if (discordBot == null) {
            await ctx.SendPrivateResponse("Cannot request account unlinking without Discord bot");
            return;
        }

        if (!ctx.Connection.Player.DiscordLinked) {
            await ctx.SendPrivateResponse("Your account is not linked with Discord. Link it using '!link' command");
            return;
        }

        DiscordLink discordLink = ctx.Connection.Player.DiscordLink;
        await discordLink.Revoke(discordBot, ctx.Connection);
        await ctx.SendPrivateResponse("Warning: your account is successfully unlinked with Discord");
    }
}
