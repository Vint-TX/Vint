using System.Reflection;
using Vint.Core.ChatCommands.Attributes;

namespace Vint.Core.ChatCommands.Commands;

public class UserCommands : ChatCommandModule {
    [ChatCommand("help", "Show list of commands or usage of specified command")]
    public void HelpCommand(
        ChatCommandContext context, 
        [Option("command", "Name of command to get help", true)] string commandName = "") {
        if (!string.IsNullOrWhiteSpace(commandName)) {
            ChatCommand? command = ChatCommandProcessor.Commands.SingleOrDefault(command => command.Info.Name == commandName);

            if (command == null) {
                context.SendResponse($"Command {commandName} not found");
                return;
            }
            
            context.SendResponse(command.ToString());
        } else {
            IEnumerable<string> commands = ChatCommandProcessor.Commands
                .Where(command => {
                    RequirePermissionsAttribute? requirePermissionsAttribute = command.Method.GetCustomAttribute<RequirePermissionsAttribute>();

                    return requirePermissionsAttribute == null ||
                           (context.Connection.Player.Groups & requirePermissionsAttribute.Permissions) == requirePermissionsAttribute.Permissions;
                })
                .Select(command => command.Info.ToString());

            string response = string.Join("\n", commands);
            context.SendResponse(response);
        }
    }
}