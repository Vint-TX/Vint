using EmbedIO;
using Serilog;
using Vint.Core.Server.API.Modules;
using Vint.Core.Server.Common.Middlewares;
using Vint.Core.Utils;
using ILogger = Serilog.ILogger;

namespace Vint.Core.Server.API;

public class ApiServer {
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
    WebSocketApiModule WebSocketApiModule { get; }

    public async Task Start() => await Server.RunAsync();

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
}
