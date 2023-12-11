using Vint.Core.Server;

namespace Vint.Core.Protocol.Commands;

public interface ICommand {
    public void Execute(IPlayerConnection connection);
}
