using System.Reflection;
using System.Web;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord.Utils;
using Vint.Core.Server;
using Vint.Core.Utils;
using ILogger = Serilog.ILogger;

namespace Vint.Core.Discord;

public class DiscordBot(
    string token,
    GameServer gameServer
) {
    const string StatusTemplate = "Vint | {0} players online";
    int LastPlayersCount { get; set; }
    ILogger Logger { get; } = Log.Logger.ForType(typeof(DiscordBot));
    DiscordClient Client { get; set; } = null!;
    DiscordGuild Guild { get; set; } = null!;
    DiscordChannel ReportsChannel { get; set; } = null!;

    public async Task Start() {
        Client = new DiscordClient(new DiscordConfiguration {
            Token = token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All
        });

        ServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton(this)
            .AddSingleton(gameServer)
            .BuildServiceProvider();

        SlashCommandsExtension commands = Client.UseSlashCommands(new SlashCommandsConfiguration { Services = serviceProvider });
        commands.RegisterCommands(Assembly.GetExecutingAssembly());

        commands.SlashCommandExecuted += (_, args) => {
            InteractionContext ctx = args.Context;
            string username = ctx.User.Username;
            string commandName = ctx.CommandName;
            string channelName = ctx.Channel.Name;
            string executionPlace = ctx.Guild == null! ? "DMs" : ctx.Guild.Name;

            Logger.Information("{User} executed command \'{Name}\' in {Place}, {Channel}", username, commandName, executionPlace, channelName);
            return Task.CompletedTask;
        };

        commands.SlashCommandErrored += (_, args) => {
            Logger.Error(args.Exception, "Got an exception while executing command {Name}", args.Context.CommandName);
            return Task.CompletedTask;
        };

        await Client.ConnectAsync(new DiscordActivity("Vint", ActivityType.Competing));

        Guild = await Client.GetGuildAsync(ConfigManager.Discord.GuildId);
        ReportsChannel = await Client.GetChannelAsync(ConfigManager.Discord.ReportsChannelId);
    }

    public void SetPlayersCount(int count) {
        if (Client is not { IsConnected: true } || LastPlayersCount == count) return;

        Task.Run(async () => await Client.UpdateStatusAsync(new DiscordActivity(string.Format(StatusTemplate, count), ActivityType.Competing)));
        LastPlayersCount = count;
    }

    public void SendReport(string message, string reporter) {
        DiscordEmbedBuilder embed = Embeds.GetNotificationEmbed(message, "New report submitted", $"Reported by **{reporter}**");

        Task.Run(async () => await ReportsChannel.SendMessageAsync(embed));
    }

    public void SendMessage(ulong userId, Action<DiscordMessageBuilder> builder) {
        DiscordMember? member = GetMember(userId);

        if (member! == null!) return;

        DiscordMessageBuilder messageBuilder = new();
        builder(messageBuilder);
        Task.Run(async () => await messageBuilder.SendAsync(await member.CreateDmChannelAsync()));
    }

    public DiscordMember? GetMember(string username) =>
        Guild.SearchMembersAsync(username)
            .GetAwaiter()
            .GetResult()
            .SingleOrDefault();

    public DiscordMember? GetMember(ulong id) {
        try {
            return Guild.GetMemberAsync(id)
                .GetAwaiter()
                .GetResult();
        } catch (Exception e) {
            Logger.Error(e, "Caught an exception while searching for discord member with id '{Id}'", id);
            return null;
        }
    }

    public void LinkPlayer(Player player) {
        string oauth2Url = ConfigManager.Discord.Oauth2Url.TrimEnd('&');

        if (string.IsNullOrWhiteSpace(oauth2Url)) return;

        byte[] stateHash = new byte[16];
        Random.Shared.NextBytes(stateHash);
        string stateHashHex = Convert.ToHexString(stateHash);

        DiscordLink discordLink = new() { Player = player, DiscordId = player.DiscordId, StateHash = stateHashHex };
        oauth2Url += $"&state={stateHashHex}";

        SendMessage(
            player.DiscordId,
            builder => builder
                .AddEmbed(Embeds.GetAccountConfirmationEmbed(player.Username))
                .AddComponents(
                    new DiscordLinkButtonComponent(
                        oauth2Url,
                        "Confirm",
                        emoji: new DiscordComponentEmoji(DiscordEmoji.FromUnicode("✅")))));

        using DbConnection db = new();
        db.Insert(discordLink);
    }
}
