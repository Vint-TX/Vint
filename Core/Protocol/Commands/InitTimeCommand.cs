using Vint.Core.Server;

namespace Vint.Core.Protocol.Commands;

public class InitTimeCommand(
    long serverTime
) : ICommand {
    public long ServerTime => serverTime;

    public void Execute(IPlayerConnection connection) => throw new NotImplementedException();

    public override string ToString() => $"InitTime command {{ ServerTime: {ServerTime} }}";
}