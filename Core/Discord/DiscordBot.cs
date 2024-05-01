using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Vint.Core.Config;
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

        ReportsChannel = await Client.GetChannelAsync(ConfigManager.Discord.ReportsChannelId);
    }

    public async Task SetPlayersCount(int count) {
        if (Client is not { IsConnected: true } || LastPlayersCount == count) return;

        await Client.UpdateStatusAsync(new DiscordActivity(string.Format(StatusTemplate, count), ActivityType.Competing));
        LastPlayersCount = count;
    }

    public async Task SendReport(string message, string reporter) {
        DiscordEmbedBuilder embed = Embeds.GetNotificationEmbed(message, "New report submitted", $"Reported by **{reporter}**");

        await ReportsChannel.SendMessageAsync(embed);
    }
}
