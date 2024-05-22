using Newtonsoft.Json;
using Serilog;
using Vint.Core.Utils;

namespace Vint.Core.Database;

public readonly record struct DatabaseConfig(
    string Host = "host",
    uint Port = 0,
    string Username = "username",
    string Password = "passwd",
    string Database = "db_name",
    string Version = "0.0.0"
) {
    public string ConnectionString => $"server={Host};port={Port};database={Database};uid={Username};pwd={Password}";

    static ILogger Logger { get; } = Log.Logger.ForType(typeof(DatabaseConfig));

    static string ConfigPath { get; } =
        Path.Combine(Directory.GetCurrentDirectory(), "Resources", "database.json");

    static DatabaseConfig Cache { get; set; }

    public static void Initialize() =>
        Cache = GetFromFile();

    public static DatabaseConfig Get() {
        if (Cache != default)
            return Cache;

        Initialize();
        return Cache;
    }

    static DatabaseConfig GetFromFile() {
        if (!File.Exists(ConfigPath)) {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(default(DatabaseConfig), Formatting.Indented));

            Logger.Error("Database config file was not found, so it was created. " +
                         "Please, fill it with the correct values and restart the server " +
                         "(press any key to close)");

            Console.ReadLine();
            Environment.Exit(0);
            return default;
        }

        DatabaseConfig config = JsonConvert.DeserializeObject<DatabaseConfig>(File.ReadAllText(ConfigPath));
        return config;
    }
}
