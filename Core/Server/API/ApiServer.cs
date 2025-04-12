using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.CompilerServices;
using EmbedIO;
using EmbedIO.WebSockets;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Serilog;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Controllers;
using Vint.Core.Server.API.DTO.Base;
using Vint.Core.Server.API.DTO.Error;
using Vint.Core.Server.API.OldDTO;
using Vint.Core.Server.Common.Middlewares;
using Vint.Core.Server.Common.Serialization;
using Vint.Core.Utils;
using ILogger = Serilog.ILogger;
using InvalidOperationException = System.InvalidOperationException;

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

        Dictionary<int, Delegate> handlers = [];

        Type[] types = Assembly.GetExecutingAssembly().GetTypes();

        if (types.Where(type => type.IsDefined(typeof(MessageIdAttribute)))
            .HasDuplicatesBy(type => type.GetCustomAttribute<MessageIdAttribute>()!.Id))
            throw new InvalidOperationException("Duplicate message id found");

        foreach (Type controllerType in types.Where(type => type.GetInterface(nameof(IApiController)) != null)) {
            object controller = ActivatorUtilities.CreateInstance(serviceProvider, controllerType);

            IEnumerable<MethodInfo> handlerInfos = controllerType
                .GetRuntimeMethods()
                .Where(method => method.IsDefined(typeof(MessageIdAttribute)));

            foreach (MethodInfo handlerInfo in handlerInfos) {
                int messageId = handlerInfo.GetCustomAttribute<MessageIdAttribute>()!.Id;

                if (handlers.ContainsKey(messageId))
                    throw new InvalidOperationException($"Duplicate message id {messageId}");

                if (!CheckForReturnType(handlerInfo.ReturnType))
                    throw new InvalidOperationException(
                        $"Invalid handler return type for '{controllerType}.{handlerInfo.Name}': '{handlerInfo.ReturnType}'\n" +
                        "Expected 'IClientDTO' or 'Task<IClientDTO>'");

                List<Type> typeArgs = handlerInfo
                    .GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToList();

                typeArgs.Add(handlerInfo.ReturnType);

                Type handlerType = GetFuncType(typeArgs.Count).MakeGenericType(typeArgs.ToArray());

                Delegate handler = handlerInfo.CreateDelegate(handlerType, controller);
                handlers.Add(messageId, handler);
            }
        }

        Handlers = handlers.ToFrozenDictionary();
        return;

        bool CheckForReturnType(Type type) {
            if (type == typeof(IClientDTO) || type == typeof(Task<IClientDTO>))
                return true;

            if (type.IsAssignableTo(typeof(IClientDTO)))
                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>)) {
                Type genericType = type.GenericTypeArguments[0];
                return genericType.IsAssignableTo(typeof(IClientDTO));
            }

            return false;
        }

        Type GetFuncType(int typeArgsCount) => typeArgsCount switch {
            1 => typeof(Func<>),
            2 => typeof(Func<,>),
            3 => typeof(Func<,,>),
            4 => typeof(Func<,,,>),
            5 => typeof(Func<,,,,>),
            6 => typeof(Func<,,,,,>),
            7 => typeof(Func<,,,,,,>),
            8 => typeof(Func<,,,,,,,>),
            9 => typeof(Func<,,,,,,,,>),
            10 => typeof(Func<,,,,,,,,,>),
            11 => typeof(Func<,,,,,,,,,,>),
            12 => typeof(Func<,,,,,,,,,,,>),
            13 => typeof(Func<,,,,,,,,,,,,>),
            14 => typeof(Func<,,,,,,,,,,,,,>),
            15 => typeof(Func<,,,,,,,,,,,,,,>),
            16 => typeof(Func<,,,,,,,,,,,,,,,>),
            17 => typeof(Func<,,,,,,,,,,,,,,,,>),
            _ => throw new ArgumentException("Too many parameters for Func<> delegate.")
        };
    }

    ILogger Logger { get; }
    ResponseJsonSerializer ResponseJsonSerializer { get; }
    FrozenDictionary<int, Delegate> Handlers { get; }

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

    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult _) {
        ILogger logger = GetLogger(context);
        int? requestId = -1;

        try {
            string rawMessage = Encoding.GetString(buffer);
            logger.Verbose("RX: {Message}", rawMessage);
            JObject message;

            try {
                message = JObject.Parse(rawMessage);
            } catch (Exception e) {
                logger.Warning(e, "Failed to parse message: {Message}", rawMessage);
                await SendAsync(context, new ErrorDTO(400, "Failed to parse message", e));
                return;
            }

            requestId = message["requestId"]?.ToObject<int>();

            if (requestId == null) {
                logger.Warning("Missing requestId in message: {Message}", message);
                await SendAsync(context, new ErrorDTO(400, "Missing requestId", null));
                return;
            }

            int? id = message["id"]?.ToObject<int>();

            if (id == null) {
                logger.Warning("Missing id in message: {Message}", message);
                await SendAsync(context, new ErrorDTO(400, "Missing id", null), requestId.Value);
                return;
            }

            if (!Handlers.TryGetValue(id.Value, out Delegate? handler)) {
                logger.Warning("Unknown id: {Id}", id);
                await SendAsync(context, new ErrorDTO(400, "Unknown id", null), requestId.Value);
                return;
            }

            JToken data = message["data"]!;

            ParameterInfo[] parameters = handler.Method.GetParameters();
            object?[] args = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++) {
                ParameterInfo parameter = parameters[i];
                JToken? token = data[parameter.Name!];

                if (token == null) {
                    logger.Error("Missing parameter '{Parameter}' in message: {Message}", parameter.Name, rawMessage);
                    await SendAsync(context, new ErrorDTO(400, $"Missing parameter '{parameter.Name}'", null), requestId.Value);
                    return;
                }

                try {
                    args[i] = token.ToObject(parameter.ParameterType);
                } catch (Exception e) {
                    logger.Error(e, "Failed to deserialize parameter '{Parameter}' in message: {Message}", parameter.Name, rawMessage);
                    await SendAsync(context, new ErrorDTO(400, $"Failed to deserialize parameter '{parameter.Name}'", e), requestId.Value);
                    return;
                }
            }

            object? result = handler.DynamicInvoke(args);
            IClientDTO clientDTO;

            switch (result) {
                case IClientDTO dto: {
                    clientDTO = dto;
                    break;
                }

                case Task task: {
                    Task<IClientDTO> dtoTask = Unsafe.As<Task<IClientDTO>>(task);
                    clientDTO = await dtoTask;
                    break;
                }

                default: {
                    string resType = result?.GetType().ToString() ?? "null";

                    logger.Error("Invalid handler return type: {Type}", resType);
                    await SendAsync(context, new ErrorDTO(500, "Invalid handler return type, this should not happen", resType), requestId.Value);
                    return;
                }
            }

            await SendAsync(context, clientDTO, requestId.Value);
        } catch (Exception e) {
            logger.Error(e, "Caught exception while processing message");
            await SendAsync(context, new ErrorDTO(500, "Internal server error", e), requestId ?? -1);
        }
    }

    async Task SendAsync(IWebSocketContext context, IClientDTO clientDTO, int requestId = -1) {
        string json = await GetJson(clientDTO, requestId);

        GetLogger(context).Verbose("TX: {Message}", json);
        await SendAsync(context, json);
    }

    async Task BroadcastAsync(IClientDTO clientDTO) {
        string json = await GetJson(clientDTO, -1);

        Logger.Verbose("(Broadcast) TX: {Message}", json);
        await BroadcastAsync(json);
    }

    async Task<string> GetJson(IClientDTO dto, int requestId) {
        int id = dto.GetType().GetCustomAttribute<MessageIdAttribute>()!.Id;
        JToken data = dto.ToJson(ResponseJsonSerializer.Serializer);
        return await ResponseJsonSerializer.Serialize(new ClientMessage(id, requestId, data));
    }

    ILogger GetLogger(IWebSocketContext context) => Logger.WithEndPoint(context);

    [UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
    record ClientMessage(
        int Id,
        int RequestId,
        JToken Data
    );
}
