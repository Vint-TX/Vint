using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;

namespace Vint.Core.Server.API.Utils;

public static class ApiNewsUtils {
    public static async Task NewsAvailable(this ApiServer apiServer, string title, string subtitle, string imageUrl, string language, string url) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new NewsAvailableData(title, subtitle, imageUrl, language, url));

    [MessageId(50)]
    [Subscriptions(Subscriptions.News)]
    public record NewsAvailableData(
        string Title,
        string Subtitle,
        string ImageUrl,
        string Language,
        string Url
    ) : IClientDTO;
}
