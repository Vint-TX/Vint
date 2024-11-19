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

        LoggerUtils.Initialize(LogEventLevel.Verbose);

        Logger = Log.Logger.ForType(typeof(Program));

        DatabaseConfig.Initialize();

        StaticServer staticServer = new(IPAddress.Any, StaticServerPort);
        GameServer gameServer = new(IPAddress.Any, GameServerPort);

        await Task.WhenAll(
            Task.Run(ConfigManager.InitializeCache),
            ConfigManager.InitializeMapInfos(),
            ConfigManager.InitializeChatCensorship(),
            Task.Run(async () => {
                await ConfigManager.InitializeNodes();
                await ConfigManager.InitializeConfigs();
                await ConfigManager.InitializeGlobalEntities();
            }));

        Extensions.RunTaskInBackground(staticServer.Start, e => {
            Logger.Fatal(e, "");
            Environment.Exit(e.HResult);
        }, true);

        Extensions.RunTaskInBackground(gameServer.Start, e => {
            Logger.Fatal(e, "");
            Environment.Exit(e.HResult);
        }, true);

        stopwatch.Stop();
        Logger.Information("Started in {Time}", stopwatch.Elapsed);

        await Task.Delay(-1);
    }
}
