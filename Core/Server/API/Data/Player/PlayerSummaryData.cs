namespace Vint.Core.Server.API.Data.Player;

public record PlayerSummaryData(
    long Id,
    string Username,
    string? Email,
    bool EmailConfirmed,
    bool NewsletterSubscribed,
    string? NewsletterUnsubscribeToken,
    string CountryCode
) {
    public static PlayerSummaryData FromPlayer(Database.Models.Player player) =>
        new(player.Id,
            player.Username,
            player.Email,
            player.EmailConfirmed,
            player.NewsletterSubscribed,
            player.NewsletterUnsubscribeToken,
            player.CountryCode);
}
