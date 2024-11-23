using System.Globalization;
using System.Text;
using EmbedIO;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Vint.Core.Server.API.Controllers;
using Vint.Core.Utils;
using ILogger = Serilog.ILogger;

namespace Vint.Core.Server.API;

public class ApiServer {
    const ushort Port = 5051;

    public ApiServer() {
        Server = new WebServer(options => options
                .WithUrlPrefix($"http://localhost:{Port}/")
                .WithMode(HttpListenerMode.Microsoft))
            .HandleHttpException(HandleHttpException)
            .HandleUnhandledException(HandleUnhandledException);

        WithController<InviteController>("/invites/");
        WithController<PlayerController>("/players/");

        Server.StateChanged += (_, e) => Logger.Debug("State changed: {Old} => {New}", e.OldState, e.NewState);
    }

    ILogger Logger { get; } = Log.Logger.ForType(typeof(ApiServer));
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

        ErrorResponse error = new(exception.Message ?? "Unknown error", exception.DataObject);
        await SerializeAndSend(context, error);
    }

    async Task HandleUnhandledException(IHttpContext context, Exception exception) {
        context.Response.StatusCode = 500;

        Type type = exception.GetType();
        string message = exception.Message;

        await SerializeAndSend(context, new ErrorResponse($"{type.FullName}: {message}", exception.Data));
    }

    void WithController<TController>(string baseRoute) where TController : WebApiController, new() =>
        Server.WithWebApi(baseRoute, SerializeAndSend, module => module.WithController<TController>());

    void WithController<TController>(string baseRoute, Action<WebApiModule> configure) where TController : WebApiController, new() =>
        Server.WithWebApi(baseRoute, SerializeAndSend, module => {
            module.WithController<TController>();
            configure(module);
        });
}

class ResponseJsonSerializer {
    public ResponseJsonSerializer() {
        JsonSerializerSettings settings = new() {
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        Serializer = JsonSerializer.CreateDefault(settings);
    }

    JsonSerializer Serializer { get; }

    public async Task<string> Serialize(object? value) {
        StringBuilder sb = new(256);
        StringWriter sw = new(sb, CultureInfo.InvariantCulture);

        await using (JsonTextWriter jsonWriter = new(sw)) {
            jsonWriter.Formatting = Serializer.Formatting;
            Serializer.Serialize(jsonWriter, value, null);
        }

        return sw.ToString();
    }
}

readonly record struct ErrorResponse(
    string Message,
    object? Data
);
