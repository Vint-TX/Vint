using System.Net;

namespace Vint.Core.Utils;

public static class IpUtils {
    static string? Token { get; } = Environment.GetEnvironmentVariable("IPINFO_TOKEN");

    public static string? GetCountryCode(IPAddress ipAddress) {
        if (Token == null) return null;

        using HttpClient client = new();

        string url = "https://ipinfo.io/";

        if (!IPAddress.IsLoopback(ipAddress))
            url += $"{ipAddress}/";

        url += $"country?token={Token}";

        string countryCode = client.GetStringAsync(url)
            .GetAwaiter()
            .GetResult();

        return countryCode.Trim();
    }
}