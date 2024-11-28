namespace Vint.Core.Server.API.DTO.Player;

public record PlayerSummaryDTO(
    long Id,
    string Username,
    string Email,
    ulong DiscordId
) {
    public static PlayerSummaryDTO FromPlayer(Database.Models.Player player) =>
        new(player.Id,
            player.Username,
            player.Email,
            player.DiscordUserId);
}
