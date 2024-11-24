using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("PromoCodeRedemptions")]
public class PromoCodeRedemption {
    [PrimaryKey(0), Identity] public long Id { get; set; }
    [PrimaryKey(1)] public required long PromoCodeId { get; init; }
    [PrimaryKey(2)] public required long PlayerId { get; init; }

    [Column] public required DateTimeOffset RedeemedAt { get; init; }
}
