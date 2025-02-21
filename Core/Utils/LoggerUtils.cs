using System.Collections.Frozen;
using System.Net;
using JetBrains.Annotations;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Swan;
using Swan.Logging;
using Vint.Core.Server.Game;
using ILogger = Serilog.ILogger;

namespace Vint.Core.Utils;

public static class LoggerUtils {
    static TemplateTheme Theme { get; } = new(new Dictionary<TemplateThemeStyle, string> {
        [TemplateThemeStyle.Text] = "\u001B[38;5;0253m",
        [TemplateThemeStyle.SecondaryText] = "\u001B[38;5;0246m",
        [TemplateThemeStyle.TertiaryText] = "\u001B[38;5;0242m",
        [TemplateThemeStyle.Invalid] = "\u001B[33;1m",
        [TemplateThemeStyle.Null] = "\u001B[38;5;0038m",
        [TemplateThemeStyle.Name] = "\u001B[38;5;0081m",
        [TemplateThemeStyle.String] = "\u001B[38;5;0216m",
        [TemplateThemeStyle.Number] = "\u001B[38;5;151m",
        [TemplateThemeStyle.Boolean] = "\u001B[38;5;0038m",
        [TemplateThemeStyle.Scalar] = "\u001B[38;5;0079m",
        [TemplateThemeStyle.LevelVerbose] = "\u001B[34m",
        [TemplateThemeStyle.LevelDebug] = "\u001b[36m",
        [TemplateThemeStyle.LevelInformation] = "\u001B[32m",
        [TemplateThemeStyle.LevelWarning] = "\u001B[33;1m",
        [TemplateThemeStyle.LevelError] = "\u001B[31;1m",
        [TemplateThemeStyle.LevelFatal] = "\u001B[31;1m"
    });

    public static LogEventLevel LogEventLevel { get; private set; }

    public static FrozenDictionary<LogLevel, LogEventLevel> SwanToSerilogLogLevels { get; } = new Dictionary<LogLevel, LogEventLevel> {
        { LogLevel.None, (LogEventLevel)(-1) },
        { LogLevel.Fatal, LogEventLevel.Fatal },
        { LogLevel.Error, LogEventLevel.Error },
        { LogLevel.Warning, LogEventLevel.Warning },
        { LogLevel.Info, LogEventLevel.Information },
        { LogLevel.Debug, LogEventLevel.Debug },
        { LogLevel.Trace, LogEventLevel.Verbose }
    }.ToFrozenDictionary();

    public static void Initialize(LogEventLevel logEventLevel) {
        const string template =
            "[{@t:HH:mm:ss.fff}] [{@l}] [{SourceContext}] {#if SessionEndpoint is not null}[{SessionEndpoint}] {#end}{#if Username is not null}[{Username}] {#end}{@m:lj}\n{@x}";

        LogEventLevel = logEventLevel;

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(new ExpressionTemplate(template, theme: Theme))
            .WriteTo.File(new ExpressionTemplate(template), "Vint.log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
            .MinimumLevel.Is(LogEventLevel)
            .CreateLogger();

        Swan.Logging.Logger.UnregisterLogger<ConsoleLogger>();
        Swan.Logging.Logger.RegisterLogger<SerilogLogger>();

        Log.Logger.ForType(typeof(LoggerUtils)).Information("Logger initialized");
    }

    public static ILogger ForType(this ILogger logger, Type type) =>
        logger.ForContext(Constants.SourceContextPropertyName, type.Name);

    public static ILogger ForType<T>(this ILogger logger) =>
        logger.ForType(typeof(T));

    public static ILogger WithEndPoint(this ILogger logger, IPEndPoint endPoint) =>
        logger.ForContext("SessionEndpoint", endPoint);

    // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    public static ILogger WithPlayer(this ILogger logger, SocketPlayerConnection player) =>
        logger
            .WithEndPoint(player.EndPoint)
            .ForContext("Username", player.Player?.Username);
}

[UsedImplicitly]
public sealed class SerilogLogger : Swan.Logging.ILogger {
    Dictionary<string, ILogger> SourceToLogger { get; } = new();
    public LogLevel LogLevel { get; } = LoggerUtils.SwanToSerilogLogLevels.First(pair => pair.Value == LoggerUtils.LogEventLevel).Key;

    public void Log(LogMessageReceivedEventArgs logEvent) {
        LogEventLevel logLevel = LoggerUtils.SwanToSerilogLogLevels[logEvent.MessageType];

        ILogger logger = SourceToLogger.GetOrAdd(logEvent.Source ?? "SWAN",
            static source => Serilog.Log.Logger.ForContext(Constants.SourceContextPropertyName, source));

        string message = logEvent.Message;
        Exception? exception = logEvent.Exception;
        object? extendedData = logEvent.ExtendedData;

        switch (logLevel) {
            case LogEventLevel.Verbose:
                logger.Verbose(exception, message, extendedData);
                break;

            case LogEventLevel.Debug:
                logger.Debug(exception, message, extendedData);
                break;

            case LogEventLevel.Information:
                logger.Information(exception, message, extendedData);
                break;

            case LogEventLevel.Warning:
                logger.Warning(exception, message, extendedData);
                break;

            case LogEventLevel.Error:
                logger.Error(exception, message, extendedData);
                break;

            case LogEventLevel.Fatal:
                logger.Fatal(exception, message, extendedData);
                break;

            default:
                throw new ArgumentOutOfRangeException(null, $"Unknown log level: {logLevel} ({logEvent.MessageType})");
        }
    }

    public void Dispose() {
        SourceToLogger.Clear();
    }
}
