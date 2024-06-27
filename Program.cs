using System.Diagnostics;
using System.Net;
using Serilog;
using Serilog.Events;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint;

abstract class Program {
    const ushort GameServerPort = 5050, StaticServerPort = 8080;

    static ILogger Logger { get; set; } = null!;

    static async Task Main() {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        LoggerUtils.Initialize(LogEventLevel.Information);

        Logger = Log.Logger.ForType(typeof(Program));

        DatabaseConfig.Initialize();

        StaticServer staticServer = new(IPAddress.Any, StaticServerPort);
        GameServer gameServer = new(IPAddress.Any, GameServerPort);

        await Task.WhenAll(
            Task.Run(ConfigManager.InitializeCache),
            Task.Run(ConfigManager.InitializeMapInfos),
            Task.Run(ConfigManager.InitializeMapModels), // You can comment this line to make the server starts faster
            Task.Run(ConfigManager.InitializeChatCensorship),
            Task.Run(() => {
                ConfigManager.InitializeNodes();
                ConfigManager.InitializeConfigs();
                ConfigManager.InitializeGlobalEntities();
            }));

        new Thread(() => staticServer.Start()) { Name = "Static Server" }.Start();

        Extensions.RunTaskInBackground(gameServer.Start, e => {
            Logger.Fatal(e, "");
            Environment.Exit(e.HResult);
        }, true);

        stopwatch.Stop();
        Logger.Information("Started in {Time}", stopwatch.Elapsed);

        await Task.Delay(-1);
    }
}
