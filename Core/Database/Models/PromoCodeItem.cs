using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("PromoCodeItems")]
public class PromoCodeItem {
    [PrimaryKey(0)] public required long PromoCodeId { get; init; }
    [PrimaryKey(1)] public required long Id { get; init; }
    [Column] public required int Quantity { get; init; }
}
