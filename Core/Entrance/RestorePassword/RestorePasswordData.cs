namespace Vint.Core.Entrance.RestorePassword;

public class RestorePasswordData(
    long playerId,
    string code
) {
    public long PlayerId { get; } = playerId;

    public string Code { get; set; } = code;
    public bool CodeValid { get; set; }
}
