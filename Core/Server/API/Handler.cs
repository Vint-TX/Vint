using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vint.Core.Server.API.Attributes;
using Vint.Core.Server.API.Data;

namespace Vint.Core.Server.API;

public class Handler(
    MethodBase method,
    object? instance,
    JsonSerializer serializer
) {
    static NullabilityInfoContext NullabilityContext { get; } = new();

    public object?[] ParseArgs(JToken? data, ApiConnection connection) {
        ParameterInfo[] parameters = method.GetParameters();
        object?[] args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++) {
            ParameterInfo parameter = parameters[i];

            bool allData = parameter.IsDefined(typeof(AllDataAttribute));
            JToken? token = allData ? data : data?[parameter.Name!];

            args[i] = GetArgument(token, serializer, parameter, connection);
        }

        return args;
    }

    public async Task<IClientDTO> ExecuteAsync(object?[] args) => method.Invoke(instance, args) switch {
        IClientDTO dto => dto,
        Task task => await Unsafe.As<Task<IClientDTO>>(task),
        _ => throw new UnreachableException("Invalid handler return type")
    };

    static object? GetArgument(JToken? token, JsonSerializer serializer, ParameterInfo parameter, ApiConnection connection) {
        if (parameter.ParameterType == typeof(ApiConnection)) // todo look for a better way to do this
            return connection;

        if (token != null)
            return token.ToObject(parameter.ParameterType, serializer);

        if (parameter.HasDefaultValue)
            return parameter.DefaultValue;

        if (IsNullable(parameter))
            return null;

        throw new ArgumentException($"Failed to parse argument, token: {token?.Path ?? "null"}", parameter.Name);
    }

    static bool IsNullable(ParameterInfo parameter) {
        if (Nullable.GetUnderlyingType(parameter.ParameterType) != null)
            return true;

        NullabilityInfo nullability = NullabilityContext.Create(parameter);
        return nullability.ReadState == NullabilityState.Nullable;
    }
}
