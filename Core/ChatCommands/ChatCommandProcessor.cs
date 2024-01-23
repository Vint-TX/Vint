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
            
            MethodInfo[] commands = commandModule
                .GetMethods()
                .Where(method => method.GetCustomAttribute<ChatCommandAttribute>() != null)
                .ToArray();

            foreach (MethodInfo command in commands) {
                ChatCommandAttribute chatCommandAttribute = command.GetCustomAttribute<ChatCommandAttribute>()!;
                
                IReadOnlyDictionary<string, OptionAttribute> parameters = command
                    .GetParameters()
                    .Where(parameter => parameter.GetCustomAttribute<OptionAttribute>() != null)
                    .ToDictionary(parameter => parameter.Name!, parameter => parameter.GetCustomAttribute<OptionAttribute>()!)
                    .AsReadOnly();

                if (parameters.Count < command.GetParameters().Length - 1) continue;
                
                ChatCommand chatCommand = new(command.Name, chatCommandAttribute, chatCommandModule, parameters, command);
                chatCommands.Add(chatCommand);
            }
        } 
        // ReSharper restore LoopCanBeConvertedToQuery

        Commands = chatCommands.AsReadOnly();
    }

    public static bool TryParseCommand(string value, out ChatCommand? chatCommand) {
        if (!value.StartsWith('/')) {
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

            context.SendResponse(checkAttribute.CheckFailedMessage);
            return;
        }
        
        string[] rawParameterValues = rawCommand[1..].Split();
        ParameterInfo[] parameterInfos = command.Method.GetParameters();
        List<object?> parameters = [context];

        if (rawParameterValues.Length > parameterInfos.Length) {
            context.SendResponse($"Too much parameters. Expected: {parameterInfos.Length - 1}, got: {rawParameterValues.Length - 1}");
            return;
        }
        
        for (int i = 1; i < parameterInfos.Length; i++) {
            ParameterInfo parameterInfo = parameterInfos[i];
            string? rawParameterValue = rawParameterValues.ElementAtOrDefault(i);

            if (rawParameterValue == null) {
                if (!parameterInfo.IsOptional) {
                    OptionAttribute optionAttribute = command.Parameters[parameterInfo.Name!];
                    context.SendResponse($"Parameter {optionAttribute.Name} not found");
                    return;
                } 
                
                parameters.Add(parameterInfo.DefaultValue);
                continue; 
            }
            
            parameters.Add(Convert.ChangeType(rawParameterValue, parameterInfo.ParameterType));
        }

        command.Method.Invoke(command.Module, parameters.ToArray());
        command.Module.AfterCommandExecution(context);
    }
}