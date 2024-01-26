using System.Reflection;
using Vint.Core.ChatCommands.Attributes;

namespace Vint.Core.ChatCommands;

public interface IChatCommandProcessor {
    public ChatCommand? GetOrDefault(string name);

    public IEnumerable<ChatCommand> GetAll();

    public bool TryParseCommand(string rawCommand, out ChatCommand? chatCommand);
}

public class ChatCommandProcessor : IChatCommandProcessor {
    List<ChatCommand> Commands { get; set; } = [];

    public ChatCommand? GetOrDefault(string name) =>
        Commands.SingleOrDefault(command => command.Info.Name == name);

    public IEnumerable<ChatCommand> GetAll() => Commands.AsReadOnly();

    public void RegisterCommands() {
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
                    this,
                    chatCommandAttribute,
                    chatCommandGroupAttribute,
                    chatCommandModule,
                    options,
                    command,
                    parameters);

                Commands.Add(chatCommand);
            }
        }
    }

    public bool TryParseCommand(string value, out ChatCommand? chatCommand) {
        if (!value.StartsWith('!')) {
            chatCommand = null;
            return false;
        }

        string commandName = value[1..].Split()[0];

        chatCommand = Commands.SingleOrDefault(command => command.Info.Name == commandName);
        return true;
    }
}