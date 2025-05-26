using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table(DbConstants.EmailConfirmations)]
public class EmailConfirmation {
    [PrimaryKey, Identity] public long Id { get; set; }

    [Column] public required long PlayerId { get; init; }
    [Column] public required string Token { get; init; }
    [Column] public required string? OldEmail { get; init; }
    [Column] public required string NewEmail { get; init; }
    [Column] public bool Used { get; set; }
    [Column] public bool Invalidated { get; set; }

    [Column] public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    [Column] public required DateTimeOffset ExpiresAt { get; init; }
    [Column] public DateTimeOffset? UsedAt { get; set; }

    [NotColumn] public string ConfirmationUrl => // todo from config
        $"https://vint.win/api/confirm-email?playerId={PlayerId}&token={Token}";
}
