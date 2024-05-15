using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Hulls")]
public class Hull {
    [NotColumn] readonly Player _player = null!;
    [Column] public required long SkinId { get; init; } // ??
    [Column] public long Xp { get; set; }
    [Column] public long Kills { get; set; }
    [Column] public long BattlesPlayed { get; set; }

    [PrimaryKey(1)] public required long Id { get; init; }

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))]
    public required Player Player {
        get => _player;
        init {
            _player = value;
            PlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PlayerId { get; private set; }
}
