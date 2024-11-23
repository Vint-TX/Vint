using System.Diagnostics;

namespace Vint.Core.Server.Game.Protocol.Commands;

public class CloseCommand(
    string reason
) : ICommand {
    public string Reason => reason;

    public Task Execute(IPlayerConnection connection, IServiceProvider serviceProvider) => throw new UnreachableException();

    public override string ToString() => $"Close command {{ Reason: '{Reason}' }}";
}
