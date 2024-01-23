using Vint.Core.Database.Models;

namespace Vint.Core.ChatCommands.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ChatCommandGroupAttribute(
    string name,
    string description,
    PlayerGroups permissions
) : Attribute {
    public string Name { get; private set; } = name;
    public string Description { get; private set; } = description;
    public PlayerGroups Permissions { get; private set; } = permissions;
}