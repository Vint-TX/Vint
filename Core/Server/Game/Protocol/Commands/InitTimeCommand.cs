namespace Vint.Core.Server.Game.Protocol.Commands;

public class InitTimeCommand(
    long serverTime
) : ICommand {
    public long ServerTime => serverTime;

    public override string ToString() => $"InitTime command {{ ServerTime: {ServerTime} }}";
}
