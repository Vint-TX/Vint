using System.Text;

namespace Vint.Core.ChatCommands.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ChatCommandAttribute(
    string name,
    string description = ""
) : Attribute {
    public string Name { get; } = name;
    public string Description { get; } = description;

    public override string ToString() {
        StringBuilder builder = new("!");

        builder.Append(Name);

        if (string.IsNullOrWhiteSpace(Description)) return builder.ToString();

        builder.Append(": ");
        builder.Append(Description);

        if (!Description.EndsWith('.')) builder.Append('.');

        return builder.ToString();
    }
}