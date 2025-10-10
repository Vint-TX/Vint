using System.Net;
using EmbedIO;
using EmbedIO.WebSockets;
using Serilog.Core;
using Vint.Core.Database.Models;
using Vint.Core.Server.Game.Connection;
using ILogger = Serilog.ILogger;

namespace Vint.Core.Logging;

public static class LoggerUtils {
    public static ILogger ForType(this ILogger logger, Type type) =>
        logger.ForContext(Constants.SourceContextPropertyName, type.Name);

    public static ILogger ForType<T>(this ILogger logger) =>
        logger.ForType(typeof(T));

    public static ILogger WithEndPoint(this ILogger logger, IPEndPoint endPoint) =>
        logger.ForContext("SessionEndpoint", endPoint);

    public static ILogger WithEndPoint(this ILogger logger, IHttpRequest request) {
        string? ip = request.Headers["X-Real-IP"];

        if (string.IsNullOrWhiteSpace(ip) || !IPEndPoint.TryParse(ip, out IPEndPoint? endPoint))
            endPoint = request.RemoteEndPoint;

        return logger.ForContext("SessionEndpoint", endPoint);
    }

    public static ILogger WithEndPoint(this ILogger logger, IWebSocketContext context) {
        string? ip = context.Headers["X-Real-IP"];

        if (string.IsNullOrWhiteSpace(ip) || !IPEndPoint.TryParse(ip, out IPEndPoint? endPoint))
            endPoint = context.RemoteEndPoint;

        return logger.ForContext("SessionEndpoint", endPoint);
    }

    // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    public static ILogger WithPlayer(this ILogger logger, SocketPlayerConnection player) =>
        logger
            .WithEndPoint(player.EndPoint)
            .ForContext("Username", player.Player?.Username);

    public static ILogger WithPlayer(this ILogger logger, Player player) =>
        logger.ForContext("Username", player.Username);
}
