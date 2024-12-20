namespace Vint.Core.Server.Game.Protocol.Commands;

public class CloseCommand(
    string reason
) : ICommand {
    public string Reason => reason;

    public override string ToString() => $"Close command {{ Reason: '{Reason}' }}";
}
