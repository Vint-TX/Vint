using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Net;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Vint.Core.Battles;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord.Utils;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.Server.Game;
using Vint.Core.Utils;
using ILogger = Serilog.ILogger;

namespace Vint.Core.Discord;

public class DiscordBot(
    IServiceProvider serviceProvider
) {
    const string DebugToken = "VINT_DISCORD_BOT_DEBUG_TOKEN",
        ProdToken = "VINT_DISCORD_BOT_PROD_TOKEN",
        DebugClientSecret = "VINT_DISCORD_BOT_DEBUG_CLIENT_SECRET",
        ProdClientSecret = "VINT_DISCORD_BOT_PROD_CLIENT_SECRET",
        StatusTemplate = "Vint | {0} players online";

    public ulong Id => Client.CurrentApplication.Id;
    public string ClientSecret { get; } = Environment.GetEnvironmentVariable(DebugClientSecret) ??
                                          Environment.GetEnvironmentVariable(ProdClientSecret)!;
    int LastPlayersCount { get; set; }
    ILogger Logger { get; } = Log.Logger.ForType<DiscordBot>();
    DiscordClient Client { get; set; } = null!;
    DiscordGuild Guild { get; set; } = null!;
    DiscordRole LinkedRole { get; set; } = null!;
    DiscordChannel ReportsChannel { get; set; } = null!;

    bool IsStarted { get; set; }

    public async Task TryStart() {
        string? token = Environment.GetEnvironmentVariable(DebugToken) ?? Environment.GetEnvironmentVariable(ProdToken);

        if (token == null)
            return;

        Client = DiscordClientBuilder
            .CreateDefault(token, DiscordIntents.All)
            .ConfigureLogging(builder => builder.AddSerilog(Logger))
            .ConfigureServices(serviceCollection => serviceCollection
                .AddSingleton(serviceProvider.GetRequiredService<GameServer>())
                .AddSingleton<IBattleProcessor, BattleProcessor>(_ => (BattleProcessor)serviceProvider.GetRequiredService<IBattleProcessor>()))
            .UseCommands((_, commands) => {
                    commands.AddCommands(Assembly.GetExecutingAssembly());
                    commands.AddProcessor<SlashCommandProcessor>();

                    commands.CommandExecuted += (_, args) => {
                        CommandContext ctx = args.Context;
                        string username = ctx.User.Username;
                        string commandName = ctx.Command.Name;
                        string channelName = ctx.Channel.Name;

                        string executionPlace = ctx.Channel.IsPrivate
                            ? "DMs"
                            : ctx.Guild!.Name;

                        Logger.Information("{User} executed command \'{Name}\' in {Place}, {Channel}",
                            username,
                            commandName,
                            executionPlace,
                            channelName);

                        return Task.CompletedTask;
                    };

                    commands.CommandErrored += (_, args) => {
                        Logger.Error(args.Exception, "Got an exception while executing command {Name}", args.Context.Command.Name);
                        return Task.CompletedTask;
                    };
                },
                new CommandsConfiguration { RegisterDefaultCommandProcessors = false })
            .DisableDefaultLogging()
            .Build();

        ConfigManager.NewLinkRequest = NewLinkRequest;

        Guild = await Client.GetGuildAsync(ConfigManager.Discord.GuildId);
        LinkedRole = await Guild.GetRoleAsync(ConfigManager.Discord.LinkedRoleId)!;
        ReportsChannel = await Client.GetChannelAsync(ConfigManager.Discord.ReportsChannelId);
        await Client.ConnectAsync(new DiscordActivity("Vint", DiscordActivityType.Competing));

        IsStarted = true;
    }

    public async Task SetPlayersCount(int count) {
        if (!IsStarted ||
            LastPlayersCount == count ||
            Client is not { AllShardsConnected: true }) return;

        await Client.UpdateStatusAsync(new DiscordActivity(string.Format(StatusTemplate, count), DiscordActivityType.Competing));
        LastPlayersCount = count;
    }

    public async Task SendReport(string message, string reporter) {
        if (!IsStarted) return;

        DiscordEmbedBuilder embed = Embeds.GetNotificationEmbed(message, "New report submitted", $"Reported by **{reporter}**");

        await ReportsChannel.SendMessageAsync(embed);
    }

    public async Task GiveLinkedRole(DiscordRestClient userClient, string username, string accessToken) {
        if (!IsStarted) return;

        DiscordMember? member = await AddToOrGetFromGuild(userClient.CurrentUser, username, accessToken);

        if (member! == null!) return;

        try {
            await member.GrantRoleAsync(LinkedRole, $"Linked account {username}");
        } catch (Exception e) {
            Logger.Error(e, "Caught an exception while granting the linked role");
        }
    }

    public async Task RevokeLinkedRole(ulong userId) {
        if (!IsStarted) return;

        try {
            DiscordMember member = await Guild.GetMemberAsync(userId);
            await member.RevokeRoleAsync(LinkedRole);
        } catch (Exception e) {
            Logger.Error(e, "Caught an exception while revoking the linked role");
        }
    }

    public async Task<DiscordMember?> AddToOrGetFromGuild(DiscordUser user, string username, string accessToken) {
        if (!IsStarted) return null;

        try {
            return await Guild.AddMemberWithRolesAsync(user, accessToken, [LinkedRole], username) ?? await Guild.GetMemberAsync(user.Id);
        } catch (NotFoundException) {
            return null;
        } catch (UnauthorizedException e) {
            Logger.Error(e, "Caught an exception while adding/getting the member");
            return null;
        }
    }

    async Task<bool> NewLinkRequest(string code, long playerId) {
        Dictionary<string, string> data = new() {
            { "grant_type", "authorization_code" },
            { "redirect_uri", ConfigManager.Discord.OAuth2Redirect },
            { "client_id", Id.ToString() },
            { "client_secret", ClientSecret },
            { "code", code }
        };

        using HttpClient httpClient = new();
        HttpResponseMessage response = await httpClient.PostAsync("https://discord.com/api/v10/oauth2/token", new FormUrlEncodedContent(data));

        if (!response.IsSuccessStatusCode) {
            Logger.Error("Invalid link request for {PlayerId}: {Error}", playerId, await response.Content.ReadAsStringAsync());
            return false;
        }

        OAuth2Data oAuth2Data = (await response.Content.ReadFromJsonAsync<OAuth2Data>())!;
        DateTimeOffset tokenExpirationDate = DateTimeOffset.UtcNow.AddSeconds(oAuth2Data.ExpiresIn - 300);

        DiscordRestClient userClient = new(new RestClientOptions(), oAuth2Data.AccessToken, TokenType.Bearer);

        await userClient.InitializeAsync();
        await using DbConnection db = new();

        ulong userId = userClient.CurrentUser.Id;

        if (db.DiscordLinks.Any(dLink => dLink.UserId == userId)) return false;

        DiscordLink discordLink = new() {
            AccessToken = oAuth2Data.AccessToken,
            RefreshToken = oAuth2Data.RefreshToken,
            TokenExpirationDate = tokenExpirationDate,
            PlayerId = playerId,
            UserId = userId
        };

        GameServer server = serviceProvider.GetRequiredService<GameServer>();
        IPlayerConnection? connection = server.PlayerConnections.Values.FirstOrDefault(conn => conn.IsOnline && conn.Player.Id == playerId);

        if (connection == null) {
            await discordLink.Revoke(this, connection);
            return false;
        }

        await db.BeginTransactionAsync();
        await db.InsertOrReplaceAsync(discordLink);

        await db
            .Players
            .Where(player => player.Id == playerId)
            .Set(player => player.DiscordUserId, userId)
            .Set(player => player.DiscordLinked, true)
            .UpdateAsync();

        await db.CommitTransactionAsync();

        await GiveLinkedRole(userClient, connection.Player.Username, oAuth2Data.AccessToken);

        if (!connection.Player.DiscordLinkRewarded) {
            foreach (LinkReward linkReward in ConfigManager.Discord.LinkRewards) {
                if (await connection.OwnsItem(linkReward.MarketEntity)) continue;

                await connection.PurchaseItem(linkReward.MarketEntity, linkReward.Amount, 0, false, false);
                await connection.Share(new NewItemNotificationTemplate().CreateRegular(connection.User, linkReward.MarketEntity, linkReward.Amount));
            }

            await db
                .Players
                .Where(player => player.Id == playerId)
                .Set(player => player.DiscordLinkRewarded, true)
                .UpdateAsync();

            connection.Player.DiscordLinkRewarded = true;
        }

        connection.Player.DiscordUserId = userId;
        connection.Player.DiscordLinked = true;
        connection.Player.DiscordLink = discordLink;
        return true;
    }

    public Uri GetOAuth2Uri(string redirectUri, string state, params string[] scopes) {
        const string baseUrl = "https://discord.com/oauth2/authorize?response_type=code&prompt=consent";

        StringBuilder builder = new(baseUrl);
        builder.Append($"&client_id={Id}");
        builder.Append($"&redirect_uri={redirectUri}");
        builder.Append($"&state={state}");
        builder.Append($"&scope={string.Join(' ', scopes)}");

        return new Uri(builder.ToString());
    }
}
