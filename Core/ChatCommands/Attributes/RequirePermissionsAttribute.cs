using Vint.Core.Database.Models;

namespace Vint.Core.ChatCommands.Attributes;

public class RequirePermissionsAttribute(
    PlayerGroups permissions
) : BaseCheckAttribute {
    public PlayerGroups Permissions { get; } = permissions;

    public override string CheckFailedMessage => "Not enough permissions to execute command";

    public override bool Check(ChatCommandContext context) =>
        (context.Connection.Player.Groups & Permissions) == Permissions;
}