using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("PromoCodeItems")]
public class PromoCodeItem {
    [PrimaryKey(1)] public required long Id { get; init; }
    [Column] public required int Quantity { get; init; }

    [Association(ThisKey = nameof(PromoCodeId), OtherKey = nameof(PromoCode.Id))] [field: NotColumn]
    public required PromoCode PromoCode {
        get;
        init {
            field = value;
            PromoCodeId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PromoCodeId { get; private init; }
}
