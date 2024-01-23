using System.Reflection;
using System.Text;
using Vint.Core.ChatCommands.Attributes;

namespace Vint.Core.ChatCommands;

public sealed class ChatCommand(
    string methodName,
    ChatCommandAttribute chatCommandAttribute,
    ChatCommandModule module,
    IReadOnlyDictionary<string, OptionAttribute> parameters,
    MethodInfo method
) {
    public string MethodName { get; private set; } = methodName;
    public ChatCommandAttribute Info { get; } = chatCommandAttribute;
    public ChatCommandModule Module { get; } = module;
    public IReadOnlyDictionary<string, OptionAttribute> Parameters { get; private set; } = parameters;
    public MethodInfo Method { get; } = method;

    public override string ToString() {
        StringBuilder builder = new(Info.ToString());
        
        builder.Append("\nUsage: /");
        builder.Append(Info.Name);
        builder.Append(' ');

        foreach (OptionAttribute option in Parameters.Values) {
            builder.Append(option);
            builder.Append(' ');
        }

        return builder.ToString();
    }
}