using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("HullSkins")]
public class HullSkin {
    [NotColumn] Player _player = null!;
    [PrimaryKey(2)] public long Id { get; init; }

    [PrimaryKey(1)] public long HullId { get; init; }

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