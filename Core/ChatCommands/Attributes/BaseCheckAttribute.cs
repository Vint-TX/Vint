namespace Vint.Core.ChatCommands.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public abstract class BaseCheckAttribute : Attribute {
    public abstract string CheckFailedMessage { get; }

    public abstract bool Check(ChatCommandContext context);
}