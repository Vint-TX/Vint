using System.Text;

namespace Vint.Core.ChatCommands.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class OptionAttribute(
    string name,
    string description = "",
    bool optional = false
) : Attribute {
    public string Name { get; } = name;
    public string Description { get; } = description;
    public bool Optional { get; } = optional;

    public override string ToString() {
        StringBuilder builder = new();

        if (Optional) {
            builder.Append('[');
            builder.Append(Name);
        } else {
            builder.Append('<');
            builder.Append(Name);
        }

        if (!string.IsNullOrWhiteSpace(Description)) {
            builder.Append(" (");
            builder.Append(Description);
            builder.Append(')');
        }

        builder.Append(Optional ? ']' : '>');
        return builder.ToString();
    }
}