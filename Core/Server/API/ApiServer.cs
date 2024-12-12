using EmbedIO;
using EmbedIO.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Vint.Core.Server.API.Controllers;
using Vint.Core.Server.API.DTO;
using Vint.Core.Server.API.Serialization;
using Vint.Core.Utils;
using ILogger = Serilog.ILogger;

namespace Vint.Core.Server.API;

public class ApiServer {
    const ushort Port = 5051;

    public ApiServer(IServiceProvider serviceProvider) {
        ServiceProvider = serviceProvider;

        Server = new WebServer(options => options
                .WithUrlPrefix($"http://localhost:{Port}/")
                .WithMode(HttpListenerMode.EmbedIO))
            .HandleHttpException(HandleHttpException)
            .HandleUnhandledException(HandleUnhandledException);

        WithController<InviteController>("/invites");
        WithController<PlayerController>("/players");
        WithController<PromoCodeController>("/promoCodes");
        WithController<BattleController>("/battles");
        WithController<ServerController>("/server");

        Server.StateChanged += (_, e) => Logger.Debug("State changed: {Old} => {New}", e.OldState, e.NewState);
    }

    ILogger Logger { get; } = Log.Logger.ForType<ApiServer>();
    IServiceProvider ServiceProvider { get; }
    WebServer Server { get; }
    ResponseJsonSerializer ResponseJsonSerializer { get; } = new();

    public async Task Start() => await Server.RunAsync();

    async Task SerializeAndSend(IHttpContext context, object? data) {
        IHttpResponse response = context.Response;
        string str = await ResponseJsonSerializer.Serialize(data);

        response.ContentType = "application/json";
        response.ContentEncoding = WebServer.Utf8NoBomEncoding;

        if (!context.TryDetermineCompression(response.ContentType, out bool compress))
            compress = true;

        await using TextWriter textWriter = context.OpenResponseText(response.ContentEncoding, false, compress);
        await textWriter.WriteAsync(str);
    }

    async Task HandleHttpException(IHttpContext context, IHttpException exception) {
        context.Response.StatusCode = exception.StatusCode;
        exception.PrepareResponse(context);

        ExceptionDTO error = new(exception.Message ?? "Unknown error", exception.DataObject);
        await SerializeAndSend(context, error);
    }

    async Task HandleUnhandledException(IHttpContext context, Exception exception) {
        context.Response.StatusCode = 500;

        Type type = exception.GetType();
        string message = exception.Message;

        await SerializeAndSend(context, new ExceptionDTO($"{type.FullName}: {message}", exception.Data));
    }

    void WithController<TController>(string baseRoute) where TController : WebApiController =>
        Server.WithWebApi(baseRoute, SerializeAndSend,
            module => module.WithController(() => ActivatorUtilities.GetServiceOrCreateInstance<TController>(ServiceProvider)));

    void WithController<TController>(string baseRoute, Action<WebApiModule> configure) where TController : WebApiController =>
        Server.WithWebApi(baseRoute, SerializeAndSend, module => {
            module.WithController(() => ActivatorUtilities.GetServiceOrCreateInstance<TController>(ServiceProvider));
            configure(module);
        });
}
