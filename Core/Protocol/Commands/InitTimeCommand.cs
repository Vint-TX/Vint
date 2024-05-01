using System.Diagnostics;
using Vint.Core.Server;

namespace Vint.Core.Protocol.Commands;

public class InitTimeCommand(
    long serverTime
) : ICommand {
    public long ServerTime => serverTime;

    public Task Execute(IPlayerConnection connection) => throw new UnreachableException();

    public override string ToString() => $"InitTime command {{ ServerTime: {ServerTime} }}";
}
