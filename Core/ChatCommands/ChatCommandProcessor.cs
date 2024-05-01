using System.Reflection;
using Serilog;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Utils;

namespace Vint.Core.ChatCommands;

public interface IChatCommandProcessor {
    public ChatCommand? GetOrDefault(string name);

    public IEnumerable<ChatCommand> GetAll();

    public bool TryParseCommand(string rawCommand, out ChatCommand? chatCommand);
}

public class ChatCommandProcessor : IChatCommandProcessor {
    ILogger Logger { get; } = Log.Logger.ForType(typeof(ChatCommandProcessor));
    List<ChatCommand> Commands { get; set; } = [];

    public ChatCommand? GetOrDefault(string name) =>
        Commands.SingleOrDefault(command => command.Info.Name == name);

    public IEnumerable<ChatCommand> GetAll() => Commands.AsReadOnly();

    public bool TryParseCommand(string value, out ChatCommand? chatCommand) {
        if (!value.StartsWith('!')) {
            chatCommand = null;
            return false;
        }

        string commandName = value[1..].Split()[0];

        chatCommand = Commands.SingleOrDefault(command => command.Info.Name == commandName);
        return true;
    }

    public void RegisterCommands() {
        Logger.Information("Generating chat commands");

        List<Type> commandModules = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsSubclassOf(typeof(ChatCommandModule)))
            .ToList();

        foreach (Type commandModule in commandModules) {
            Logger.Debug("Generating {Name} group", commandModule.Name);

            ChatCommandModule chatCommandModule = (ChatCommandModule)Activator.CreateInstance(commandModule)!;
            ChatCommandGroupAttribute? chatCommandGroupAttribute = commandModule.GetCustomAttribute<ChatCommandGroupAttribute>();

            List<MethodInfo> commands = commandModule
                .GetMethods()
                .Where(method => method.ReturnType == typeof(Task))
                .Where(method => method.GetCustomAttribute<ChatCommandAttribute>() != null)
                .ToList();

            foreach (MethodInfo command in commands) {
                Logger.Verbose("Generating {Name} command", command.Name);

                ChatCommandAttribute chatCommandAttribute = command.GetCustomAttribute<ChatCommandAttribute>()!;
                Logger.Verbose("Method name: {Method}, command name: {Command}", command.Name, chatCommandAttribute.Name);

                IReadOnlyDictionary<string, OptionAttribute> options = command
                    .GetParameters()
                    .Where(parameter => parameter.GetCustomAttribute<OptionAttribute>() != null)
                    .ToDictionary(parameter => parameter.Name!, parameter => parameter.GetCustomAttribute<OptionAttribute>()!)
                    .AsReadOnly();

                List<ParameterInfo> parameters = command
                    .GetParameters()
                    .Skip(1) // Context
                    .ToList();

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

        Logger.Information("Chat commands generated");
    }
}
