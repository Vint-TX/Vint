using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Reflection;
using EmbedIO.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Vint.Core.Logging;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Controllers;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.Status;
using Vint.Core.Server.Common.Serialization;

namespace Vint.Core.Server.API;

public class WebSocketApiModule : WebSocketModule {
    public WebSocketApiModule(IServiceProvider serviceProvider, string urlPath) : base(urlPath, true) {
        EnsureSchemaValid();

        Logger = Log.Logger.ForType<WebSocketApiModule>();
        ResponseJsonSerializer = new ResponseJsonSerializer();
        RequestSerializer = new JsonSerializer {
            Converters = {
                new StrictEnumConverter<Subscriptions>() // todo better way to do this
            }
        };

        Dictionary<int, Handler> handlers = [];

        IEnumerable<Type> controllers = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.GetInterface(nameof(IApiController)) != null);

        foreach (Type controllerType in controllers) {
            object controller = ActivatorUtilities.CreateInstance(serviceProvider, controllerType);

            IEnumerable<MethodInfo> handlerInfos = controllerType
                .GetRuntimeMethods()
                .Where(method => method.IsDefined(typeof(MessageIdAttribute)));

            foreach (MethodInfo handler in handlerInfos) {
                int messageId = handler.GetCustomAttribute<MessageIdAttribute>()!.Id;
                handlers.Add(messageId, new Handler(handler, controller, RequestSerializer));
            }
        }

        Handlers = handlers.ToFrozenDictionary();
    }

    ILogger Logger { get; }
    ResponseJsonSerializer ResponseJsonSerializer { get; }
    JsonSerializer RequestSerializer { get; }
    FrozenDictionary<int, Handler> Handlers { get; }

    ConcurrentDictionary<string, ApiConnection> Connections { get; } = [];

    protected override async Task OnClientConnectedAsync(IWebSocketContext context) {
        ILogger logger = GetLogger(context);

        if (!Connections.TryAdd(context.Id, new ApiConnection(context))) {
            logger.Error("Client with Id '{Id}' is already connected", context.Id);
            await CloseAsync(context);
            return;
        }

        logger.Information("({Id}) New client connected", context.Id);
    }

    protected override Task OnClientDisconnectedAsync(IWebSocketContext context) {
        ILogger logger = GetLogger(context);
        Connections.TryRemove(context.Id, out _);
        logger.Information("({Id}) Client disconnected", context.Id);
        return Task.CompletedTask;
    }

    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult _) {
        ILogger logger = GetLogger(context);
        int? requestId = -1;

        try {
            if (!Connections.TryGetValue(context.Id, out ApiConnection? connection)) {
                logger.Error("Client with Id '{Id}' is not connected", context.Id);
                await CloseAsync(context);
                return;
            }

            string rawMessage = Encoding.GetString(buffer);
            logger.Verbose("RX: {Message}", rawMessage);
            JObject message;

            try {
                message = JObject.Parse(rawMessage);
            } catch (Exception e) {
                logger.Error(e, "Failed to parse message: {Message}", rawMessage);
                await SendAsync(context, ErrorDTO.BadRequest("Failed to parse message", e));
                return;
            }

            requestId = message["requestId"]?.ToObject<int>();

            if (requestId == null) {
                logger.Error("Missing requestId in message: {Message}", message);
                await SendAsync(context, ErrorDTO.BadRequest("Missing requestId"));
                return;
            }

            int? id = message["id"]?.ToObject<int>();

            if (id == null) {
                logger.Error("Missing id in message: {Message}", message);
                await SendAsync(context, ErrorDTO.BadRequest("Missing id"), requestId.Value);
                return;
            }

            if (!Handlers.TryGetValue(id.Value, out Handler? handler)) {
                logger.Error("Unknown id: {Id}", id);
                await SendAsync(context, ErrorDTO.BadRequest("Unknown id"), requestId.Value);
                return;
            }

            object?[] args;

            try {
                JToken? data = message["data"];
                args = handler.ParseArgs(data, connection);
            } catch (Exception e) {
                logger.Error(e, "Failed to parse arguments");
                await SendAsync(context, ErrorDTO.BadRequest("Failed to parse arguments", e), requestId.Value);
                return;
            }

            IClientDTO clientDTO = await handler.ExecuteAsync(args);
            await SendAsync(context, clientDTO, requestId.Value);
        } catch (Exception e) {
            logger.Error(e, "Caught exception while processing message");
            await SendAsync(context, ErrorDTO.InternalServerError("Internal server error", e), requestId ?? -1);
        }
    }

    public async Task BroadcastAsync(IClientDTO clientDTO) {
        Subscriptions? subscriptions = clientDTO.GetType().GetCustomAttribute<SubscriptionsAttribute>()?.Subscriptions;

        if (subscriptions.HasValue) {
            await BroadcastAsync(clientDTO, subscriptions.Value);
            return;
        }

        string json = await GetJson(clientDTO, -1);

        Logger.Verbose("(Broadcast) TX: {Message}", json);
        await BroadcastAsync(json);
    }

    public async Task BroadcastAsync(IClientDTO clientDTO, Subscriptions subscriptions) {
        string json = await GetJson(clientDTO, -1);

        Logger.Verbose("(Broadcast {Subscriptions}) TX: {Message}", subscriptions, json);
        await Task.WhenAll(Connections.Values
            .Where(conn => conn.Subscriptions.HasFlag(subscriptions))
            .Select(conn => SendAsync(conn.Context, json)));
    }

    async Task SendAsync(IWebSocketContext context, IClientDTO clientDTO, int requestId = -1) {
        string json = await GetJson(clientDTO, requestId);

        GetLogger(context).Verbose("TX: {Message}", json);
        await SendAsync(context, json);
    }

    async Task<string> GetJson(IClientDTO dto, int requestId) {
        int id = dto.GetType().GetCustomAttribute<MessageIdAttribute>()!.Id;
        return await ResponseJsonSerializer.Serialize(new ClientMessage(id, requestId, dto));
    }

    ILogger GetLogger(IWebSocketContext context) => Logger.WithEndPoint(context);

    [Conditional("DEBUG")]
    static void EnsureSchemaValid() {
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();

        List<Type> noMessageId = types
            .Where(type => !type.IsAbstract && type.GetInterface(nameof(IClientDTO)) != null)
            .Where(dto => !dto.IsDefined(typeof(MessageIdAttribute)))
            .ToList();

        if (noMessageId.Count != 0) {
            string str = string.Join(", ", noMessageId.Select(type => type.Name));
            throw new InvalidOperationException($"DTOs without MessageId: {str}");
        }

        List<Type> messageIdTypes = types.Where(type => type.IsDefined(typeof(MessageIdAttribute))).ToList();
        List<Type> noDto = messageIdTypes.Where(messageId => messageId.GetInterface(nameof(IClientDTO)) == null).ToList();

        if (noDto.Count != 0) {
            string str = string.Join(", ", noDto.Select(type => type.Name));
            throw new InvalidOperationException($"MessageIds without DTO: {str}");
        }

        List<MethodInfo> handlers = types
            .Where(type => type.GetInterface(nameof(IApiController)) != null)
            .SelectMany(controller => controller.GetRuntimeMethods())
            .Where(method => method.IsDefined(typeof(MessageIdAttribute)))
            .ToList();

        List<MethodInfo> invalidHandlers = handlers
            .Where(handler => !CheckForReturnType(handler.ReturnType))
            .ToList();

        if (invalidHandlers.Count != 0) {
            string str = string.Join(", ", invalidHandlers.Select(handler => handler.Name));
            throw new InvalidOperationException($"Invalid handler return type: {str}");
        }

        List<MemberInfo> duplicates = handlers
            .Cast<MemberInfo>()
            .Concat(messageIdTypes)
            .GroupBy(member => member.GetCustomAttribute<MessageIdAttribute>()!.Id)
            .Where(group => group.Count() > 1)
            .SelectMany(group => group)
            .ToList();

        if (duplicates.Count != 0) {
            string str = string.Join(", ", duplicates.Select(member => member.Name));
            throw new InvalidOperationException($"Duplicate MessageId: {str}");
        }

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
    }
}
