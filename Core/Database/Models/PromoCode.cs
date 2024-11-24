using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("PromoCodes")]
public class PromoCode {
    [PrimaryKey, Identity] public long Id { get; set; }
    [Column] public required string Code { get; init; }
    [Column] public int Uses { get; set; } = 0;

    [Column] public int MaxUses { get; init; } = -1;
    [Column] public long OwnedPlayerId { get; init; } = -1;
    [Column] public DateTimeOffset? ExpiresAt { get; init; } = null;

    [NotColumn] public bool CanBeUsed => (MaxUses == -1 || Uses < MaxUses) &&
                                         (!ExpiresAt.HasValue || ExpiresAt > DateTimeOffset.UtcNow);
    public bool CanBeUsedBy(long playerId) => OwnedPlayerId == -1 || playerId == OwnedPlayerId;
}
