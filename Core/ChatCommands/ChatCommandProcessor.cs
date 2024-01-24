using System.Reflection;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.ECS.Entities;
using Vint.Core.Server;

namespace Vint.Core.ChatCommands;

public static class ChatCommandProcessor {
    public static IReadOnlyList<ChatCommand> Commands { get; private set; } = null!;

    public static void RegisterCommands() {
        List<ChatCommand> chatCommands = [];

        Type[] commandModules = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsSubclassOf(typeof(ChatCommandModule)))
            .ToArray();

        // ReSharper disable LoopCanBeConvertedToQuery
        foreach (Type commandModule in commandModules) {
            ChatCommandModule chatCommandModule = (ChatCommandModule)Activator.CreateInstance(commandModule)!;
            ChatCommandGroupAttribute? chatCommandGroupAttribute = commandModule.GetCustomAttribute<ChatCommandGroupAttribute>();

            MethodInfo[] commands = commandModule
                .GetMethods()
                .Where(method => method.GetCustomAttribute<ChatCommandAttribute>() != null)
                .ToArray();

            foreach (MethodInfo command in commands) {
                ChatCommandAttribute chatCommandAttribute = command.GetCustomAttribute<ChatCommandAttribute>()!;

                IReadOnlyDictionary<string, OptionAttribute> options = command
                    .GetParameters()
                    .Where(parameter => parameter.GetCustomAttribute<OptionAttribute>() != null)
                    .ToDictionary(parameter => parameter.Name!, parameter => parameter.GetCustomAttribute<OptionAttribute>()!)
                    .AsReadOnly();

                ParameterInfo[] parameters = command
                    .GetParameters()
                    .Skip(1) // Context
                    .ToArray();

                if (options.Count < command.GetParameters().Length - 1) continue;

                ChatCommand chatCommand = new(command.Name,
                    chatCommandAttribute,
                    chatCommandGroupAttribute,
                    chatCommandModule,
                    options,
                    command,
                    parameters);

                chatCommands.Add(chatCommand);
            }
        }
        // ReSharper restore LoopCanBeConvertedToQuery

        Commands = chatCommands.AsReadOnly();
    }

    public static bool TryParseCommand(string value, out ChatCommand? chatCommand) {
        if (!value.StartsWith('!')) {
            chatCommand = null;
            return false;
        }

        string commandName = value[1..].Split()[0];

        chatCommand = Commands.SingleOrDefault(command => command.Info.Name == commandName);
        return true;
    }

    public static void Execute(IPlayerConnection connection, IEntity chat, string rawCommand, ChatCommand command) {
        ChatCommandContext context = new(connection, chat, command.Info);
        bool shouldExecute = command.Module.BeforeCommandExecution(context);

        if (!shouldExecute) return;

        foreach (BaseCheckAttribute checkAttribute in command.Method.GetCustomAttributes<BaseCheckAttribute>()) {
            if (checkAttribute.Check(context)) continue;

            context.SendPrivateResponse(checkAttribute.CheckFailedMessage);
            return;
        }

        if (command.Method.GetCustomAttribute<RequirePermissionsAttribute>() == null &&
            command.ChatCommandGroupAttribute != null &&
            (context.Connection.Player.Groups & command.ChatCommandGroupAttribute.Permissions) != command.ChatCommandGroupAttribute.Permissions) {
            context.SendPrivateResponse("Not enough permissions to execute command");
            return;
        }

        List<object?> parameters = [context];
        string[] rawParameterValues = rawCommand
            .Split()
            .Skip(1) // Command name
            .ToArray();

        if (rawParameterValues.Length > command.Parameters.Length &&
            command.Parameters.All(param => param.GetCustomAttribute<WaitingForTextAttribute>() == null)) {
            context.SendPrivateResponse($"Too much parameters. Expected: {command.Parameters.Length}, got: {rawParameterValues.Length}");
            return;
        }

        for (int i = 0; i < command.Parameters.Length; i++) {
            ParameterInfo parameterInfo = command.Parameters[i];
            string? rawParameterValue = rawParameterValues.ElementAtOrDefault(i);

            if (rawParameterValue == null) {
                if (!parameterInfo.IsOptional) {
                    OptionAttribute optionAttribute = command.Options[parameterInfo.Name!];
                    context.SendPrivateResponse($"Parameter '{optionAttribute.Name}' not found");
                    return;
                }

                parameters.Add(parameterInfo.DefaultValue);
                continue;
            }

            if (parameterInfo.GetCustomAttribute<WaitingForTextAttribute>() != null) {
                rawParameterValue = string.Join(' ', rawParameterValues.Skip(i));
                i = command.Parameters.Length;
            }

            try {
                parameters.Add(Convert.ChangeType(rawParameterValue, parameterInfo.ParameterType));
            } catch (FormatException) {
                context.SendPrivateResponse(
                    $"Unexpected '{command.Options[parameterInfo.Name!]}' parameter type. Expected: {parameterInfo.ParameterType.Name}");
                return;
            }
        }

        try {
            command.Method.Invoke(command.Module, parameters.ToArray());
            command.Module.AfterCommandExecution(context);
        } catch (TargetParameterCountException) {
            context.SendPrivateResponse($"Too few parameters. Expected: {command.Parameters.Length}, got: {parameters.Count - 1}");
        } catch (TargetInvocationException invocationException) {
            if (invocationException.InnerException is NotImplementedException)
                context.SendPrivateResponse($"Handler for '{command.Info.Name}' command is not implemented yet");
            else throw;
        }
    }
}