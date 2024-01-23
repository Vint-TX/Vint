using System.Reflection;
using Vint.Core.ChatCommands.Attributes;
using Vint.Core.Database.Models;

namespace Vint.Core.ChatCommands.Commands;

[ChatCommandGroup("user", "Commands for all players", PlayerGroups.None)]
public class UserModule : ChatCommandModule {
    [ChatCommand("help", "Show list of commands or usage of specified command")]
    public void HelpCommand(
        ChatCommandContext ctx,
        [Option("command", "Name of command to get help", true)]
        string commandName = "") {
        if (!string.IsNullOrWhiteSpace(commandName)) {
            ChatCommand? command = ChatCommandProcessor.Commands.SingleOrDefault(command => command.Info.Name == commandName);

            RequirePermissionsAttribute? requirePermissionsAttribute = command?.Method.GetCustomAttribute<RequirePermissionsAttribute>();
            ChatCommandGroupAttribute? chatCommandGroupAttribute = command?.ChatCommandGroupAttribute;

            if (command == null ||
                requirePermissionsAttribute != null &&
                (ctx.Connection.Player.Groups & requirePermissionsAttribute.Permissions) != requirePermissionsAttribute.Permissions ||
                chatCommandGroupAttribute != null &&
                (ctx.Connection.Player.Groups & chatCommandGroupAttribute.Permissions) != chatCommandGroupAttribute.Permissions) {
                ctx.SendPrivateResponse($"Command '{commandName}' not found");
                return;
            }

            ctx.SendPrivateResponse(command.ToString());
        } else {
            IEnumerable<string> commands = ChatCommandProcessor.Commands
                .Where(command => {
                    RequirePermissionsAttribute? requirePermissionsAttribute = command.Method.GetCustomAttribute<RequirePermissionsAttribute>();

                    if (requirePermissionsAttribute != null)
                        return (ctx.Connection.Player.Groups & requirePermissionsAttribute.Permissions) == requirePermissionsAttribute.Permissions;

                    ChatCommandGroupAttribute? chatCommandGroupAttribute = command.ChatCommandGroupAttribute;

                    if (chatCommandGroupAttribute != null)
                        return (ctx.Connection.Player.Groups & chatCommandGroupAttribute.Permissions) == chatCommandGroupAttribute.Permissions;

                    return true;
                })
                .Select(command => command.Info.ToString());

            string response = string.Join("\n", commands);
            ctx.SendPrivateResponse(response);
        }
    }
}