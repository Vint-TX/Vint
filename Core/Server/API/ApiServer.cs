using EmbedIO;
using Microsoft.Extensions.Hosting;
using Serilog;
using Vint.Core.Logging;
using Vint.Core.Server.Common.Middlewares;
using ILogger = Serilog.ILogger;

namespace Vint.Core.Server.API;

public class ApiServer : BackgroundService {
    const ushort Port = 5051;

    public ApiServer(IServiceProvider serviceProvider) {
        WebSocketApiModule = new WebSocketApiModule(serviceProvider, "/");

        Server = new WebServer(options => options
                .WithUrlPrefix($"http://localhost:{Port}/")
                .WithMode(HttpListenerMode.EmbedIO))
            .WithModule(new LoggingModule<ApiServer>("/"))
            .WithModule(WebSocketApiModule)
            .HandleHttpException(HandleHttpException)
            .HandleUnhandledException(HandleUnhandledException);

        Server.StateChanged += (_, e) => Logger.Information("State changed: {Old} => {New}", e.OldState, e.NewState);
    }

    ILogger Logger { get; } = Log.Logger.ForType<ApiServer>();
    WebServer Server { get; }

    public WebSocketApiModule WebSocketApiModule { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await Server.RunAsync(stoppingToken);

    Task HandleHttpException(IHttpContext context, IHttpException exception) {
        ILogger logger = Logger.WithEndPoint(context.Request);

        if (exception is Exception e) logger.Error(e, "HTTP exception");
        else logger.Error("HTTP exception: {Message}", exception.Message);
        return Task.CompletedTask;
    }

    Task HandleUnhandledException(IHttpContext context, Exception exception) {
        Logger.WithEndPoint(context.Request).Error(exception, "Unhandled exception");
        return Task.CompletedTask;
    }

    public override void Dispose() {
        base.Dispose();

        WebSocketApiModule.Dispose();
        Server.Dispose();

        GC.SuppressFinalize(this);
    }
}
