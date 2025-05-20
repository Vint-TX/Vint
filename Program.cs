using EmbedIO.Net;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Vint.Core.Battle.Lobby;
using Vint.Core.Chat.Commands;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Database.Migrations;
using Vint.Core.Matchmaking;
using Vint.Core.Quests;
using Vint.Core.Server;
using Vint.Core.Server.API;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol;
using Vint.Core.Server.Static;
using Vint.Core.Utils;

namespace Vint;

abstract class Program {
    static async Task Main() {
        LoggerUtils.Initialize(LogEventLevel.Information);

        Log.Logger
            .ForType<Program>()
            .Information("Welcome to Vint!");

        EndPointManager.UseIpv6 = false;
        DatabaseConfig.Initialize();

        await Task.WhenAll(
            Task.Run(ConfigManager.InitializeCache),
            ConfigManager.InitializeMapInfos(),
            ConfigManager.InitializeChatCensorship(),
            Task.Run(async () => {
                await ConfigManager.InitializeNodes();
                await ConfigManager.InitializeConfigs();
                await ConfigManager.InitializeGlobalEntities();
            }));

        IServiceProvider serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder.AddSerilog())
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddMySql8()
                .WithGlobalConnectionString(DatabaseConfig.Get().ConnectionString)
                .ScanIn(typeof(Initial).Assembly).For.Migrations()
            )
            .Configure<RunnerOptions>(opt => opt.AllowBreakingChange = false) // SET TO TRUE ONLY IF YOU'RE SURE THAT YOU WANT TO ALLOW BREAKING CHANGES TO THE DATABASE!
            .AddSingleton<ApiServer>()
            .AddSingleton<StaticServer>()
            .AddSingleton<GameServer>()
            .AddSingleton<Runner>()
            .AddSingleton<Protocol>()
            .AddSingleton<QuestManager>()
            .AddSingleton<LobbyProcessor>()
            .AddSingleton<RatingMatchmakingProcessor>()
            .AddSingleton<ArcadeMatchmakingProcessor>()
            .AddSingleton<IChatCommandProcessor>(serviceProvider => {
                ChatCommandProcessor chatCommandProcessor = new(serviceProvider);
                chatCommandProcessor.RegisterCommands();
                return chatCommandProcessor;
            })
            .BuildServiceProvider();

        Runner runner = serviceProvider.GetRequiredService<Runner>();
        await runner.Run();
    }
}
