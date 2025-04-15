namespace Vint.Core.Server.API.Data.Player;

public record PlayerSummaryData(
    long Id,
    string Username,
    string Email,
    ulong DiscordId
) {
    public static PlayerSummaryData FromPlayer(Database.Models.Player player) =>
        new(player.Id,
            player.Username,
            player.Email,
            player.DiscordUserId);
}
