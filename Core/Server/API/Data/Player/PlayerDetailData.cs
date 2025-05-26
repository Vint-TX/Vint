namespace Vint.Core.Server.API.Data.Player;

public record PlayerDetailData(
    long Id,
    string Username,
    string? Email,
    bool EmailConfirmed,
    bool NewsletterSubscribed,
    string? NewsletterUnsubscribeToken,
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
    public static PlayerDetailData FromPlayer(Database.Models.Player player) =>
        new(player.Id,
            player.Username,
            player.Email,
            player.EmailConfirmed,
            player.NewsletterSubscribed,
            player.NewsletterUnsubscribeToken,
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
