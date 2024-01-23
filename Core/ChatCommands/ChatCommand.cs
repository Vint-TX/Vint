using System.Reflection;
using System.Text;
using Vint.Core.ChatCommands.Attributes;

namespace Vint.Core.ChatCommands;

public sealed class ChatCommand(
    string methodName,
    ChatCommandAttribute chatCommandAttribute,
    ChatCommandGroupAttribute? chatCommandGroupAttribute,
    ChatCommandModule module,
    IReadOnlyDictionary<string, OptionAttribute> options,
    MethodInfo method,
    ParameterInfo[] parameters
) {
    public string MethodName { get; } = methodName;
    public ChatCommandAttribute Info { get; } = chatCommandAttribute;
    public ChatCommandGroupAttribute? ChatCommandGroupAttribute { get; } = chatCommandGroupAttribute;
    public ChatCommandModule Module { get; } = module;
    public IReadOnlyDictionary<string, OptionAttribute> Options { get; } = options;
    public MethodInfo Method { get; } = method;
    public ParameterInfo[] Parameters { get; } = parameters;

    public override string ToString() {
        StringBuilder builder = new(Info.ToString());

        builder.Append("\nUsage: !");
        builder.Append(Info.Name);
        builder.Append(' ');

        foreach (OptionAttribute option in Options.Values) {
            builder.Append(option);
            builder.Append(' ');
        }

        return builder.ToString();
    }
}