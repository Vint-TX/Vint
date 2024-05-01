using System.Reflection;
using System.Text;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;

namespace Vint.Core.ChatCommands;

public sealed class ChatCommand(
    string methodName,
    IChatCommandProcessor processor,
    ChatCommandAttribute chatCommandAttribute,
    ChatCommandGroupAttribute? chatCommandGroupAttribute,
    ChatCommandModule module,
    IReadOnlyDictionary<string, OptionAttribute> options,
    MethodInfo method,
    List<ParameterInfo> parameters
) {
    public string MethodName { get; } = methodName;
    public IChatCommandProcessor Processor { get; } = processor;
    public ChatCommandAttribute Info { get; } = chatCommandAttribute;
    public ChatCommandGroupAttribute? ChatCommandGroupAttribute { get; } = chatCommandGroupAttribute;
    public ChatCommandModule Module { get; } = module;
    public IReadOnlyDictionary<string, OptionAttribute> Options { get; } = options;
    public MethodInfo Method { get; } = method;
    public List<ParameterInfo> Parameters { get; } = parameters;

    public async Task Execute(IPlayerConnection connection, IEntity chat, string rawCommand) {
        ChatCommandContext context = new(Processor, connection, chat, Info);
        bool shouldExecute = Module.BeforeCommandExecution(context);

        if (!shouldExecute) return;

        foreach (BaseCheckAttribute checkAttribute in Method.GetCustomAttributes<BaseCheckAttribute>()) {
            if (checkAttribute.Check(context)) continue;

            await context.SendPrivateResponse(checkAttribute.CheckFailedMessage);
            return;
        }

        if (Method.GetCustomAttribute<RequirePermissionsAttribute>() == null &&
            ChatCommandGroupAttribute != null &&
            (context.Connection.Player.Groups & ChatCommandGroupAttribute.Permissions) != ChatCommandGroupAttribute.Permissions) {
            await context.SendPrivateResponse("Not enough permissions to execute command");
            return;
        }

        List<object?> parameters = [context];
        List<string> rawParameterValues = rawCommand
            .Split()
            .Skip(1) // Command name
            .ToList();

        if (rawParameterValues.Count > Parameters.Count &&
            Parameters.All(param => param.GetCustomAttribute<WaitingForTextAttribute>() == null)) {
            await context.SendPrivateResponse($"Too much parameters. Expected: {Parameters.Count}, got: {rawParameterValues.Count}");
            return;
        }

        for (int i = 0; i < Parameters.Count; i++) {
            ParameterInfo parameterInfo = Parameters[i];
            string? rawParameterValue = rawParameterValues.ElementAtOrDefault(i);

            if (rawParameterValue == null) {
                if (!parameterInfo.IsOptional) {
                    OptionAttribute optionAttribute = Options[parameterInfo.Name!];
                    await context.SendPrivateResponse($"Parameter '{optionAttribute.Name}' not found");
                    return;
                }

                parameters.Add(parameterInfo.DefaultValue);
                continue;
            }

            if (parameterInfo.GetCustomAttribute<WaitingForTextAttribute>() != null) {
                rawParameterValue = string.Join(' ', rawParameterValues.Skip(i));
                i = Parameters.Count;
            }

            try {
                object? param;

                if (parameterInfo.ParameterType.IsEnum) {
                    if (!Enum.TryParse(parameterInfo.ParameterType, rawParameterValue, true, out param))
                        param = null;
                } else
                    param = Convert.ChangeType(rawParameterValue, parameterInfo.ParameterType);

                parameters.Add(param);
            } catch (FormatException e) {
                OptionAttribute option = Options[parameterInfo.Name!];

                if (e.InnerException is OverflowException) {
                    object? minValue = parameterInfo.ParameterType.GetField("MinValue")?.GetValue(null);
                    object? maxValue = parameterInfo.ParameterType.GetField("MaxValue")?.GetValue(null);

                    await context.SendPrivateResponse($"'{option}' must be in range from '{minValue}' to '{maxValue}'");
                    return;
                }

                await context.SendPrivateResponse($"Unexpected '{option}' parameter type. Expected: {parameterInfo.ParameterType.Name}");
                return;
            }
        }

        try {
            await (Task)Method.Invoke(Module, parameters.ToArray())!;
            Module.AfterCommandExecution(context);
        } catch (TargetParameterCountException) {
            await context.SendPrivateResponse($"Too few parameters. Expected: {Parameters.Count}, got: {parameters.Count - 1}");
        } catch (TargetInvocationException invocationException) {
            if (invocationException.InnerException is NotImplementedException)
                await context.SendPrivateResponse($"Handler for '{Info.Name}' command is not implemented yet");
            else throw;
        }
    }

    public override string ToString() {
        StringBuilder builder = new(Info.ToString());

        builder.Append("\nUsage: !");
        builder.Append(Info.Name);
        builder.Append(' ');

        foreach (OptionAttribute option in Options.Values) {
            builder.Append(option);
            builder.Append(' ');
        }

        return builder.ToString();
    }
}
