using System.Net;
using Serilog;
using Vint.Core.Config;
using Vint.Core.Server;
using Vint.Utils;

namespace Vint;

abstract class Program {
    static ILogger Logger { get; set; } = null!;

    static async Task Main() {
        LoggerUtils.Initialize();

        Logger = Log.Logger.ForType(typeof(Program));

        StaticServer staticServer = new(IPAddress.Any, 8080);
        GameServer gameServer = new(IPAddress.Any, 5050);

        ClientConfigGenerator.InitializeCache();

        new Thread(() => staticServer.Start()) { Name = "Static Server" }.Start();
        new Thread(() => gameServer.Start()) { Name = "Game Server" }.Start();

        await Task.Delay(-1);
    }
}
