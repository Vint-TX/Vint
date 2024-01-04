using System.Net;
using System.Reflection;
using LinqToDB;
using LinqToDB.Mapping;
using LinqToDB.SqlQuery;
using Serilog;
using Serilog.Events;
using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.Server;
using Vint.Core.Utils;

namespace Vint;

abstract class Program {
    static ILogger Logger { get; set; } = null!;

    static Task Main() {
        LoggerUtils.Initialize(LogEventLevel.Verbose);

        Logger = Log.Logger.ForType(typeof(Program));

        DatabaseConfig.Initialize();

        //RecreateTables();

        StaticServer staticServer = new(IPAddress.Any, 8080);
        GameServer gameServer = new(IPAddress.Any, 5050);

        ConfigManager.InitializeCache();
        ConfigManager.InitializeNodes();
        ConfigManager.InitializeGlobalEntities();
        ConfigManager.InitializeMapInfos();

        new Thread(() => staticServer.Start()) { Name = "Static Server" }.Start();
        new Thread(() => gameServer.Start()) { Name = "Game Server" }.Start();

        return Task.Delay(-1);
    }

    static void RecreateTables() {
        List<Type> types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsDefined(typeof(TableAttribute)))
            .ToList();

        MethodInfo dropTable = typeof(DataExtensions).GetMethod(nameof(DataExtensions.DropTable),
            1,
            [typeof(IDataContext), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(string), typeof(TableOptions)])!;

        MethodInfo createTable = typeof(DataExtensions).GetMethod(nameof(DataExtensions.CreateTable))!;

        using DbConnection db = new();

        foreach (Type type in types) {
            dropTable.MakeGenericMethod(type)
                .Invoke(null, [db, null, null, null, false, null, TableOptions.DropIfExists]);

            createTable.MakeGenericMethod(type)
                .Invoke(null, [db, null, null, null, null, null, DefaultNullable.None, null, TableOptions.CreateIfNotExists]);
        }
    }
}