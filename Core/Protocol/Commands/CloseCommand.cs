using Vint.Core.Server;

namespace Vint.Core.Protocol.Commands;

public class CloseCommand(
    string reason
) : ICommand {
    public string Reason => reason;

    public void Execute(PlayerConnection connection) => throw new NotImplementedException();
}