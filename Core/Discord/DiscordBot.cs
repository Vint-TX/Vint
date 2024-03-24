using System.Reflection;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
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

        CommandsExtension commands = Client.UseCommands(new CommandsConfiguration { ServiceProvider = serviceProvider });
        commands.AddCommands(Assembly.GetExecutingAssembly());
        await commands.AddProcessorAsync(new SlashCommandProcessor());

        commands.CommandExecuted += (_, args) => {
            CommandContext ctx = args.Context;
            string username = ctx.User.Username;
            string commandName = ctx.Command.Name;
            string channelName = ctx.Channel.Name;
            string executionPlace = ctx.Guild! == null! ? "DMs" : ctx.Guild.Name;

            Logger.Information("{User} executed command \'{Name}\' in {Place}, {Channel}", username, commandName, executionPlace, channelName);
            return Task.CompletedTask;
        };

        commands.CommandErrored += (_, args) => {
            Logger.Error(args.Exception, "Got an exception while executing command {Name}", args.Context.Command.Name);
            return Task.CompletedTask;
        };

        await Client.ConnectAsync(new DiscordActivity("Vint", ActivityType.Competing));
    }

    public void SetPlayersCount(int count) {
        if (Client is not { IsConnected: true } || LastPlayersCount == count) return;

        Task.Run(async () => await Client.UpdateStatusAsync(new DiscordActivity(string.Format(StatusTemplate, count), ActivityType.Competing)));
        LastPlayersCount = count;
    }

    public async void SendReport(string report) {
        DiscordEmbedBuilder embed = Embeds.GetWarningEmbed(report);
        
        //TODO(Kurays): Change later this shit 
        DiscordChannel channel = await Client.GetChannelAsync(1196058513693216838);
        await channel.SendMessageAsync(embed);
    }
}