namespace Vint.Core.Server.API.DTO.Player;

public record PlayerDetailDTO(
    long Id,
    string Username,
    string Email,
    ulong DiscordId,
    bool Subscribed,
    string CountryCode,
    long AvatarId,
    long Crystals,
    long XCrystals,
    long Experience,
    int Rank,
    uint Reputation,
    string League,
    string Fraction,
    DateTimeOffset RegistrationTime,
    DateTimeOffset LastLoginTime
) {
    public static PlayerDetailDTO FromPlayer(Database.Models.Player player) =>
        new(player.Id,
            player.Username,
            player.Email,
            player.DiscordUserId,
            player.Subscribed,
            player.CountryCode,
            player.CurrentAvatarId,
            player.Crystals,
            player.XCrystals,
            player.Experience,
            player.Rank,
            player.Reputation,
            player.League.ToString(),
            player.FractionName,
            player.RegistrationTime,
            player.LastLoginTime);
}
