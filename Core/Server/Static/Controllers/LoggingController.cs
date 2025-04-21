using EmbedIO;
using EmbedIO.WebApi;
using LinqToDB;
using Newtonsoft.Json;
using Serilog;
using Vint.Core.Database;
using Vint.Core.Database.Models;
using Vint.Core.Discord;
using Vint.Core.Server.API;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.Common.Attributes.Methods;
using Vint.Core.Utils;

namespace Vint.Core.Server.Static.Controllers;

public class LoggingController(
    DiscordBot discordBot,
    ApiServer apiServer
) : WebApiController {
    [Post("/")]
    public async Task ReceiveLog() { // todo microservice
        if (!Request.HasEntityBody)
            throw HttpException.BadRequest();

        string log = await HttpContext.GetRequestBodyAsStringAsync();

        if (string.IsNullOrWhiteSpace(log))
            throw HttpException.BadRequest();

        int startIndex = log.LastIndexOf('{');
        int endIndex = log.LastIndexOf('}');
        int length = endIndex - startIndex + 1;

        if (startIndex == -1 || endIndex == -1 || length <= 0)
            throw HttpException.BadRequest();

        string json = log.Substring(startIndex, length);

        try {
            ClientLogDTO dto = JsonConvert.DeserializeObject<ClientLogDTO>(json);

            ClientLog clientLog = new() {
                Timestamp = DateTimeOffset.UtcNow,
                LogLevel = dto.Level,
                Username = dto.Username,
                Hostname = dto.Host,
                DeviceId = dto.DeviceId,
                OperatingSystem = dto.OS,
                ClientVersion = dto.ClientVersion,
                InitUrl = dto.InitUrl,
                SessionId = dto.SessionId,
                Message = dto.Message,
                ExceptionMessage = dto.Exception,
                RawLog = log
            };

            await using (DbConnection db = new())
                clientLog.Id = await db.InsertWithInt64IdentityAsync(clientLog);

            await discordBot.SendLog(clientLog.Username, clientLog.Message, log, clientLog.Id, clientLog.Timestamp);
            await apiServer.WebSocketApiModule.BroadcastAsync(new NewLogDTO(dto, clientLog.Timestamp, log));
        } catch (Exception e) {
            Log.Logger.ForType<LoggingController>().WithEndPoint(Request).Error(e, "Failed to deserialize client log");

            string filePath = await SaveLogOnDisk(log);
            await discordBot.SendLog($"Failed to deserialize, saved on disk: {filePath}", "", log, -1, DateTimeOffset.UtcNow);
            await apiServer.WebSocketApiModule.BroadcastAsync(new FailedLogDTO(filePath, DateTimeOffset.UtcNow, log));
        }
    }

    readonly record struct ClientLogDTO(
        ClientLogLevel Level = ClientLogLevel.All,
        string Username = "",
        string Host = "",
        string DeviceId = "",
        string OS = "",
        string ClientVersion = "",
        string InitUrl = "",
        long SessionId = 0,
        string Message = "",
        string Exception = ""
    );

    [MessageId(37)]
    [Subscriptions(Subscriptions.Logs)]
    record NewLogDTO(
        ClientLogDTO ClientLog,
        DateTimeOffset Timestamp,
        string RawLog
    ) : IClientDTO;

    [MessageId(38)]
    [Subscriptions(Subscriptions.Logs)]
    record FailedLogDTO(
        string FilePath,
        DateTimeOffset Timestamp,
        string RawLog
    ) : IClientDTO;

    static async Task<string> SaveLogOnDisk(string log) {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "ClientLogs");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        string fileName = $"{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss-fffffff}.log";
        string filePath = Path.Combine(path, fileName);
        await File.WriteAllTextAsync(filePath, log);
        return filePath;
    }
}
