using Vint.Core.Logging;

namespace Vint.Core.Chat.Commands;

public abstract class ChatCommandModule {
    public virtual bool BeforeCommandExecution(ChatCommandContext context) {
        context.Connection.Logger
            .ForType(GetType())
            .Information("Trying to execute command '{Name}'", context.CommandInfo.Name);

        return true;
    }

    public virtual void AfterCommandExecution(ChatCommandContext context) { }
}
