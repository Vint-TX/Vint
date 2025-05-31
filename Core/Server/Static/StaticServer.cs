using System.Net;
using EmbedIO;
using EmbedIO.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Vint.Core.Logging;
using Vint.Core.Server.Common.Middlewares;
using Vint.Core.Server.Static.Controllers;

namespace Vint.Core.Server.Static;

public class StaticServer : BackgroundService {
    const ushort Port = 8080;

    public StaticServer(IServiceProvider serviceProvider) {
        ServiceProvider = serviceProvider;

        Server = new WebServer(options => options
                .WithUrlPrefix($"http://*:{Port}/")
                .WithMode(HttpListenerMode.EmbedIO))
            .WithModule(new LoggingModule<StaticServer>("/"))
            .HandleHttpException(HandleHttpException)
            .HandleUnhandledException(HandleUnhandledException);

        WithController<StateController>("/state");
        WithController<InitController>("/init");
        WithController<ConfigController>("/config");
        WithController<LoggingController>("/log");

        Server.StateChanged += (_, e) => Logger.Information("State changed: {Old} => {New}", e.OldState, e.NewState);
    }

    ILogger Logger { get; } = Log.Logger.ForType<StaticServer>();
    IServiceProvider ServiceProvider { get; }
    WebServer Server { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await Server.RunAsync(stoppingToken);

    Task HandleHttpException(IHttpContext context, IHttpException exception) {
        HttpStatusCode status = (HttpStatusCode)exception.StatusCode;

        if (status is not HttpStatusCode.BadRequest and not HttpStatusCode.NotFound) {
            ILogger logger = Logger.WithEndPoint(context.Request);

            if (exception is Exception e) logger.Error(e, "HTTP exception");
            else logger.Error("HTTP exception: {Message}", exception.Message);
        }

        context.Response.StatusCode = exception.StatusCode;
        exception.PrepareResponse(context);
        return Task.CompletedTask;
    }

    Task HandleUnhandledException(IHttpContext context, Exception exception) {
        Logger.WithEndPoint(context.Request).Error(exception, "Unhandled exception");

        context.Response.StatusCode = 500;
        return Task.CompletedTask;
    }

    void WithController<TController>(string baseRoute) where TController : WebApiController =>
        Server.WithWebApi(baseRoute, ResponseSerializer.None(false), module =>
            module.WithController(() => ActivatorUtilities.GetServiceOrCreateInstance<TController>(ServiceProvider)));

    void WithController<TController>(string baseRoute, Action<WebApiModule> configure) where TController : WebApiController =>
        Server.WithWebApi(baseRoute, ResponseSerializer.None(false), module => {
            module.WithController(() => ActivatorUtilities.GetServiceOrCreateInstance<TController>(ServiceProvider));
            configure(module);
        });

    public override void Dispose() {
        base.Dispose();

        Server.Dispose();
        GC.SuppressFinalize(this);
    }
}
