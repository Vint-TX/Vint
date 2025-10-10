using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using LinqToDB;
using LinqToDB.DataProvider.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Vint.Core.Battle.Autopilot;
using Vint.Core.Battle.Lobby;
using Vint.Core.Chat.Commands;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Matchmaking;
using Vint.Core.Quests;
using Vint.Core.Server.API;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol;
using Vint.Core.Server.Static;
using Vint.Core.Utils;

EmbedIO.Net.EndPointManager.UseIpv6 = false;
Swan.Logging.Logger.UnregisterLogger<Swan.Logging.ConsoleLogger>();

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings {
    Args = args
});

string dbConnectionString = builder.Configuration.GetConnectionString("Vint")!;
DbConnection.DefaultOptions = new DataOptions<DbConnection>(GetDataOptions());
RunMigrations();

builder.Services
    .AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration))
    .AddTransient<BotBuilder>()
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
    .AddHostedSingletonService<ApiServer>()
    .AddHostedSingletonService<StaticServer>()
    .AddHostedSingletonService<GameServer>();

using IHost host = builder.Build();

await Task.WhenAll(
    Task.Run(ConfigManager.InitializeCache),
    ConfigManager.InitializeMapInfos(),
    ConfigManager.InitializeChatCensorship(),
    Task.Run(async () => {
        await ConfigManager.InitializeNodes();
        await ConfigManager.InitializeConfigs();
        await ConfigManager.InitializeGlobalEntities();
    }));

await host.RunAsync();
return;

void RunMigrations() {
    IServiceProvider serviceProvider = new ServiceCollection()
        .AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration))
        .AddFluentMigratorCore()
        .ConfigureRunner(runnerBuilder => runnerBuilder
            .AddMySql8()
            .WithGlobalConnectionString(dbConnectionString)
            .ScanIn(Assembly.GetExecutingAssembly()).For.Migrations())
        .Configure<RunnerOptions>(opt => opt.AllowBreakingChange = false) // SET TO TRUE ONLY IF YOU'RE SURE THAT YOU WANT TO ALLOW BREAKING CHANGES TO THE DATABASE!
        .BuildServiceProvider();

    using IServiceScope scope = serviceProvider.CreateScope();
    IMigrationRunner migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    migrationRunner.MigrateUp();
}

DataOptions GetDataOptions() =>
    new DataOptions()
        .UseMySql(dbConnectionString, MySqlVersion.MariaDB10);
