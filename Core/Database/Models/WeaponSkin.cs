using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("WeaponSkins")]
public class WeaponSkin {
    [PrimaryKey(2)] public required long Id { get; init; }

    [PrimaryKey(1)] public required long WeaponId { get; init; }

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))]
    public required Player Player {
        get;
        init {
            field = value;
            PlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PlayerId { get; private set; }
}
