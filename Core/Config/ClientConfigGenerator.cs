using System.Diagnostics.CodeAnalysis;
using Serilog;
using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.GZip;
using Vint.Utils;

namespace Vint.Core.Config;

public static class ClientConfigGenerator {
    static string ConfigsPath { get; } =
        Path.Combine(Directory.GetCurrentDirectory(), "Resources", "StaticServer", "config");
    static ILogger Logger { get; } = Log.Logger.ForType(typeof(ClientConfigGenerator));
    static Dictionary<string, byte[]> LocaleToConfigCache { get; } = new(2);

    public static void InitializeCache() {
        Logger.Information("Generating archives");

        foreach (string configDir in Directory.EnumerateDirectories(ConfigsPath)) {
            string locale = new DirectoryInfo(configDir).Name;

            Logger.Debug("Generating archive for the '{Locale}' locale", locale);

            using MemoryStream outStream = new();

            using (IWriter writer = WriterFactory.Open(outStream, ArchiveType.Tar, new GZipWriterOptions())) {
                writer.WriteAll(configDir, "*", SearchOption.AllDirectories);
            }

            byte[] buffer = outStream.ToArray();

            LocaleToConfigCache[locale] = buffer;
        }
    }

    public static bool TryGetConfig(string locale, [NotNullWhen(true)] out byte[]? config) =>
        LocaleToConfigCache.TryGetValue(locale, out config);
}
