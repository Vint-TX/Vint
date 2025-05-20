using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;

namespace Vint.Core.Server.API.Utils;

public static class ApiReportUtils {
    public static async Task Report(this ApiServer apiServer, string message, string reporter) =>
        await apiServer.WebSocketApiModule.BroadcastAsync(new ReportData(message, reporter));

    [MessageId(39)]
    [Subscriptions(Subscriptions.Reports)]
    record ReportData(
        string Message,
        string Reporter
    ) : IClientDTO;
}
