using Newtonsoft.Json;

namespace Vint.Core.Config;

public class ServerConfig {
    const string FileName = "server.json";
    public static string FilePath { get; } = Path.Combine(ConfigManager.ResourcesPath, FileName);

    public uint SeasonNumber { get; set; }
    public DateTimeOffset LastQuestsUpdate { get; set; }

    public async Task Save() =>
        await File.WriteAllTextAsync(FilePath, JsonConvert.SerializeObject(this));
}
