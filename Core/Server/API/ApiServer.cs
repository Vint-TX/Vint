using System.Collections.Frozen;
using System.Reflection;
using EmbedIO;
using EmbedIO.WebSockets;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.Error;
using Vint.Core.Server.API.DTO;
using Vint.Core.Server.Common.Middlewares;
using Vint.Core.Server.Common.Serialization;
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
        ILogger logger = Logger.WithEndPoint(context.Request);

        if (exception is Exception e) logger.Error(e, "HTTP exception");
        else logger.Error("HTTP exception: {Message}", exception.Message);

        context.Response.StatusCode = exception.StatusCode;
        exception.PrepareResponse(context);

        ExceptionDTO error = new(exception.Message ?? "Unknown error", exception.DataObject);
        await SerializeAndSend(context, error);
    }

    async Task HandleUnhandledException(IHttpContext context, Exception exception) {
        Logger.WithEndPoint(context.Request).Error(exception, "Unhandled exception");

        context.Response.StatusCode = 500;

        Type type = exception.GetType();
        string message = exception.Message;

        await SerializeAndSend(context, new ExceptionDTO($"{type.FullName}: {message}", exception.Data));
    }
}

public class WebSocketApiModule : WebSocketModule {
    public WebSocketApiModule(IServiceProvider serviceProvider, string urlPath) : base(urlPath, true) {
        Logger = Log.Logger.ForType<WebSocketApiModule>();
        ResponseJsonSerializer = new ResponseJsonSerializer();

        Types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.GetInterface(nameof(IServerData)) != null)
            .ToFrozenDictionary(type => type.GetCustomAttribute<MessageIdAttribute>()!.Id);

        RequestJsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings {
            ContractResolver = new DependencyInjectionContractResolver(serviceProvider)
        });
    }

    ILogger Logger { get; }
    JsonSerializer RequestJsonSerializer { get; }
    ResponseJsonSerializer ResponseJsonSerializer { get; }
    FrozenDictionary<int, Type> Types { get; }

    protected override Task OnClientConnectedAsync(IWebSocketContext context) {
        ILogger logger = GetLogger(context);
        logger.Information("New client connected");
        return Task.CompletedTask;
    }

    protected override Task OnClientDisconnectedAsync(IWebSocketContext context) {
        ILogger logger = GetLogger(context);
        logger.Information("Client disconnected");
        return Task.CompletedTask;
    }

    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result) {
        ILogger logger = GetLogger(context);

        try {
            string rawMessage = Encoding.GetString(buffer);
            logger.Verbose("RX: {Message}", rawMessage);
            JObject message;

            try {
                message = JObject.Parse(rawMessage);
            } catch (Exception e) {
                logger.Warning(e, "Failed to parse message: {Message}", rawMessage);
                await SendAsync(context, new ErrorData(400, "Failed to parse message", e));
                return;
            }

            int? requestId = message["requestId"]?.ToObject<int>(RequestJsonSerializer);

            if (requestId == null) {
                logger.Warning("Missing requestId in message: {Message}", message);
                await SendAsync(context, new ErrorData(400, "Missing requestId", null));
                return;
            }

            int? id = message["id"]?.ToObject<int>(RequestJsonSerializer);

            if (id == null) {
                logger.Warning("Missing id in message: {Message}", message);
                await SendAsync(context, new ErrorData(400, "Missing id", null), requestId.Value);
                return;
            }

            if (!Types.TryGetValue(id.Value, out Type? type)) {
                logger.Warning("Unknown id: {Id}", id);
                await SendAsync(context, new ErrorData(400, "Unknown id", null), requestId.Value);
                return;
            }

            if (message["data"]?.ToObject(type, RequestJsonSerializer) is not IServerData serverData) {
                logger.Warning("Failed to deserialize data");
                await SendAsync(context, new ErrorData(400, "Failed to deserialize data", null), requestId.Value);
                return;
            }

            IClientData clientData = await serverData.Process();
            await SendAsync(context, clientData, requestId.Value);
        } catch (Exception e) {
            logger.Error(e, "Caught exception while processing message");
            await SendAsync(context, new ErrorData(500, "Internal server error", e));
        }
    }

    async Task SendAsync(IWebSocketContext context, IClientData clientData, int requestId = -1) {
        int id = clientData.GetType().GetCustomAttribute<MessageIdAttribute>()!.Id;
        string json = await ResponseJsonSerializer.Serialize(new ClientMessage(id, requestId, clientData));

        GetLogger(context).Verbose("TX: {Message}", json);
        await SendAsync(context, json);
    }

    async Task BroadcastAsync(IClientData clientData) {
        int id = clientData.GetType().GetCustomAttribute<MessageIdAttribute>()!.Id;
        string json = await ResponseJsonSerializer.Serialize(new ClientMessage(id, -1, clientData));

        Logger.Verbose("(Broadcast) TX: {Message}", json);
        await BroadcastAsync(json);
    }

    ILogger GetLogger(IWebSocketContext context) => Logger.WithEndPoint(context);

    [UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
    record ClientMessage(
        int Id,
        int RequestId,
        IClientData Data
    );
}
