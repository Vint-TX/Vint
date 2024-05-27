using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord.Utils;
using Vint.Core.ECS.Events.Notification;
using Vint.Core.ECS.Templates.Notification;
using Vint.Core.Server;
using Vint.Core.Utils;
using ILogger = Serilog.ILogger;

namespace Vint.Core.Discord;

public class DiscordBot(
    string token,
    GameServer gameServer
) {
    const string ClientSecretProdEnv = "VINT_DISCORD_BOT_PROD_CLIENT_SECRET";
    const string ClientSecretDebugEnv = "VINT_DISCORD_BOT_DEBUG_CLIENT_SECRET";
    const string StatusTemplate = "Vint | {0} players online";

    public ulong Id => Client.CurrentApplication.Id;
    public string ClientSecret { get; } = Environment.GetEnvironmentVariable(ClientSecretDebugEnv) ??
                                          Environment.GetEnvironmentVariable(ClientSecretProdEnv)!;
    int LastPlayersCount { get; set; }
    ILogger Logger { get; } = Log.Logger.ForType(typeof(DiscordBot));
    DiscordClient Client { get; set; } = null!;
    DiscordGuild Guild { get; set; } = null!;
    DiscordRole LinkedRole { get; set; } = null!;
    DiscordChannel ReportsChannel { get; set; } = null!;

    public async Task Start() {
        Client = new DiscordClient(new DiscordConfiguration {
            Token = token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All
        });

        ServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder.AddSerilog(Logger))
            .AddSingleton(this)
            .AddSingleton(gameServer)
            .BuildServiceProvider();

        CommandsExtension commands = Client.UseCommands(new CommandsConfiguration {
            ServiceProvider = serviceProvider,
            RegisterDefaultCommandProcessors = false
        });

        SlashCommandProcessor slashCommandProcessor = new();

        await commands.AddProcessorAsync(slashCommandProcessor);
        commands.AddCommands(Assembly.GetExecutingAssembly());

        commands.CommandExecuted += (_, args) => {
            CommandContext ctx = args.Context;
            string username = ctx.User.Username;
            string commandName = ctx.Command.Name;
            string channelName = ctx.Channel.Name;
            string executionPlace = ctx.Channel.IsPrivate ? "DMs" : ctx.Guild!.Name;

            Logger.Information("{User} executed command \'{Name}\' in {Place}, {Channel}", username, commandName, executionPlace, channelName);
            return Task.CompletedTask;
        };

        commands.CommandErrored += (_, args) => {
            Logger.Error(args.Exception, "Got an exception while executing command {Name}", args.Context.Command.Name);
            return Task.CompletedTask;
        };

        ConfigManager.NewLinkRequest = NewLinkRequest;

        Guild = await Client.GetGuildAsync(ConfigManager.Discord.GuildId);
        LinkedRole = Guild.GetRole(ConfigManager.Discord.LinkedRoleId)!;
        ReportsChannel = await Client.GetChannelAsync(ConfigManager.Discord.ReportsChannelId);
        await Client.ConnectAsync(new DiscordActivity("Vint", DiscordActivityType.Competing));
    }

    public async Task SetPlayersCount(int count) {
        if (LastPlayersCount == count || Client is not { IsConnected: true }) return;

        await Client.UpdateStatusAsync(new DiscordActivity(string.Format(StatusTemplate, count), DiscordActivityType.Competing));
        LastPlayersCount = count;
    }

    public async Task SendReport(string message, string reporter) {
        DiscordEmbedBuilder embed = Embeds.GetNotificationEmbed(message, "New report submitted", $"Reported by **{reporter}**");

        await ReportsChannel.SendMessageAsync(embed);
    }

    public async Task GiveLinkedRole(DiscordClient userClient, string username, string accessToken) {
        DiscordMember? member = await AddToOrGetFromGuild(userClient.CurrentUser, username, accessToken);

        if (member! == null!) return;

        try {
            await member.GrantRoleAsync(LinkedRole, $"Linked account {username}");
        } catch (Exception e) {
            Logger.Error(e, "Caught an exception while granting the linked role");
        }
    }

    public async Task RevokeLinkedRole(ulong userId) {
        try {
            DiscordMember member = await Guild.GetMemberAsync(userId);
            await member.RevokeRoleAsync(LinkedRole);
        } catch (Exception e) {
            Logger.Error(e, "Caught an exception while revoking the linked role");
        }
    }

    public async Task<DiscordMember?> AddToOrGetFromGuild(DiscordUser user, string username, string accessToken) {
        try {
            return await Guild.AddMemberWithRolesAsync(user, accessToken, [LinkedRole], username) ??
                   await Guild.GetMemberAsync(user.Id);
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

        if (!response.IsSuccessStatusCode) return false;

        OAuth2Data oAuth2Data = (await response.Content.ReadFromJsonAsync<OAuth2Data>())!;
        DateTimeOffset tokenExpirationDate = DateTimeOffset.UtcNow.AddSeconds(oAuth2Data.ExpiresIn - 300);

        DiscordClient userClient = new(new DiscordConfiguration {
            Token = oAuth2Data.AccessToken,
            TokenType = TokenType.Bearer
        });

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

        IPlayerConnection? connection = gameServer.PlayerConnections.Values.FirstOrDefault(conn => conn.IsOnline && conn.Player.Id == playerId);

        if (connection == null) {
            await discordLink.Revoke(this, connection);
            return false;
        }

        await db.BeginTransactionAsync();
        await db.InsertOrReplaceAsync(discordLink);

        await db.Players.Where(player => player.Id == playerId)
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

            foreach (Notification notification in connection.Notifications) {
                await connection.UnshareIfShared(notification.Entity);
                connection.Notifications.TryRemove(notification);
            }

            await connection.Send(new ShowNotificationGroupEvent(ConfigManager.Discord.LinkRewards.Count), connection.User);

            await db.Players.Where(player => player.Id == playerId)
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
