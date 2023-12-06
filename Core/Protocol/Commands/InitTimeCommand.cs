using Vint.Core.Server;

namespace Vint.Core.Protocol.Commands;

public class InitTimeCommand(long serverTime) : ICommand {
    public long ServerTime => serverTime;

    public void Execute(PlayerConnection connection) => throw new NotImplementedException();
}