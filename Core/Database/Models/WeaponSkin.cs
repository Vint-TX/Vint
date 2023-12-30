using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("WeaponSkins")]
public class WeaponSkin {
    [NotColumn] Player _player = null!;
    [PrimaryKey(2)] public long Id { get; init; }

    [PrimaryKey(1)] public long WeaponId { get; init; }

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))]
    public Player Player {
        get => _player;
        set {
            _player = value;
            PlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PlayerId { get; private set; }
}