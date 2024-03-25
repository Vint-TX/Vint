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

    static Task Main() {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        
        LoggerUtils.Initialize(LogEventLevel.Information);

        Logger = Log.Logger.ForType(typeof(Program));

        DatabaseConfig.Initialize();

        StaticServer staticServer = new(IPAddress.Any, StaticServerPort);
        GameServer gameServer = new(IPAddress.Any, GameServerPort);

        ConfigManager.InitializeCache();
        ConfigManager.InitializeNodes();
        ConfigManager.InitializeMapInfos();
        //ConfigManager.InitializeMapModels();
        ConfigManager.InitializeGlobalEntities();

        new Thread(() => staticServer.Start()) { Name = "Static Server" }.Start();
        new Thread(() => gameServer.Start()) { Name = "Game Server" }.Start();

        stopwatch.Stop();
        Logger.Information("Started in {Time}", stopwatch.Elapsed);
        
        return Task.Delay(-1);
    }
}