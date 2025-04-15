using System.Collections.Frozen;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using EmbedIO.WebSockets;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Serilog;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Controllers;
using Vint.Core.Server.API.Data;
using Vint.Core.Server.API.Data.Status;
using Vint.Core.Server.Common.Serialization;
using Vint.Core.Utils;

namespace Vint.Core.Server.API.Modules;

public class WebSocketApiModule : WebSocketModule {
    public WebSocketApiModule(IServiceProvider serviceProvider, string urlPath) : base(urlPath, true) {
        EnsureSchemaValid();

        Logger = Log.Logger.ForType<WebSocketApiModule>();
        ResponseJsonSerializer = new ResponseJsonSerializer();

        Dictionary<int, Delegate> handlers = [];

        IEnumerable<Type> controllers = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.GetInterface(nameof(IApiController)) != null);

        foreach (Type controllerType in controllers) {
            object controller = ActivatorUtilities.CreateInstance(serviceProvider, controllerType);

            IEnumerable<MethodInfo> handlerInfos = controllerType
                .GetRuntimeMethods()
                .Where(method => method.IsDefined(typeof(MessageIdAttribute)));

            foreach (MethodInfo handlerInfo in handlerInfos) {
                int messageId = handlerInfo.GetCustomAttribute<MessageIdAttribute>()!.Id;

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
    }

    ILogger Logger { get; }
    ResponseJsonSerializer ResponseJsonSerializer { get; }
    FrozenDictionary<int, Delegate> Handlers { get; }

    protected override Task OnClientConnectedAsync(IWebSocketContext context) {
        GetLogger(context).Information("New client connected");
        return Task.CompletedTask;
    }

    protected override Task OnClientDisconnectedAsync(IWebSocketContext context) {
        GetLogger(context).Information("Client disconnected");
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
                await SendAsync(context, ErrorDTO.BadRequest("Failed to parse message", e));
                return;
            }

            requestId = message["requestId"]?.ToObject<int>();

            if (requestId == null) {
                logger.Warning("Missing requestId in message: {Message}", message);
                await SendAsync(context, ErrorDTO.BadRequest("Missing requestId"));
                return;
            }

            int? id = message["id"]?.ToObject<int>();

            if (id == null) {
                logger.Warning("Missing id in message: {Message}", message);
                await SendAsync(context, ErrorDTO.BadRequest("Missing id"), requestId.Value);
                return;
            }

            if (!Handlers.TryGetValue(id.Value, out Delegate? handler)) {
                logger.Warning("Unknown id: {Id}", id);
                await SendAsync(context, ErrorDTO.BadRequest("Unknown id"), requestId.Value);
                return;
            }

            JToken data = message["data"]!;

            ParameterInfo[] parameters = handler.Method.GetParameters();
            object?[] args = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++) {
                ParameterInfo parameter = parameters[i];
                JToken? token = data[parameter.Name!];
                object? value;

                if (token == null) {
                    if (parameter.HasDefaultValue)
                        value = parameter.DefaultValue;
                    else {
                        logger.Error("Missing parameter '{Parameter}' in message: {Message}", parameter.Name, rawMessage);
                        await SendAsync(context, ErrorDTO.BadRequest($"Missing parameter '{parameter.Name}'"), requestId.Value);
                        return;
                    }
                } else
                    value = token.ToObject(parameter.ParameterType);

                try {
                    args[i] = value;
                } catch (Exception e) {
                    logger.Error(e, "Failed to deserialize parameter '{Parameter}' in message: {Message}", parameter.Name, rawMessage);
                    await SendAsync(context, ErrorDTO.BadRequest($"Failed to deserialize parameter '{parameter.Name}'", e), requestId.Value);
                    return;
                }
            }

            IClientDTO clientDTO;

            try {
                object? result = handler.DynamicInvoke(args);

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
                        await SendAsync(context, ErrorDTO.InternalServerError("Invalid handler return type, this should not happen", resType), requestId.Value);
                        return;
                    }
                }
            } catch (Exception e) {
                logger.Error(e, "Caught exception while executing handler");
                await SendAsync(context, ErrorDTO.InternalServerError("Internal server error", e), requestId.Value);
                return;
            }

            await SendAsync(context, clientDTO, requestId.Value);
        } catch (Exception e) {
            logger.Error(e, "Caught exception while processing message");
            await SendAsync(context, ErrorDTO.InternalServerError("Internal server error", e), requestId ?? -1);
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
        return await ResponseJsonSerializer.Serialize(new ClientMessage(id, requestId, dto));
    }

    ILogger GetLogger(IWebSocketContext context) => Logger.WithEndPoint(context);

    [UsedImplicitly(ImplicitUseKindFlags.Access, ImplicitUseTargetFlags.Members)]
    record ClientMessage(
        int Id,
        int RequestId,
        IClientDTO Data
    );

    static Type GetFuncType(int typeArgsCount) => typeArgsCount switch {
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
        _ => throw new ArgumentException("Too many parameters for Func<> delegate")
    };

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
