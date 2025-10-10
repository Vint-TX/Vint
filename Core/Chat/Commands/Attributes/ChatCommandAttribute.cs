using System.Text;
using JetBrains.Annotations;

namespace Vint.Core.Chat.Commands.Attributes;

[AttributeUsage(AttributeTargets.Method), MeansImplicitUse(ImplicitUseKindFlags.Access)]
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
