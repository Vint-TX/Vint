namespace Vint.Core.ChatCommands;

public abstract class ChatCommandModule {
    public virtual bool BeforeCommandExecution(ChatCommandContext context) => true;

    public virtual void AfterCommandExecution(ChatCommandContext context) { }
}