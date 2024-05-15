using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Weapons")]
public class Weapon {
    [NotColumn] readonly Player _player = null!;
    [PrimaryKey(1)] public required long Id { get; init; }

    [Column] public required long SkinId { get; init; } // ??
    [Column] public required long ShellId { get; init; } // ??
    [Column] public long Xp { get; set; }
    [Column] public long Kills { get; set; }
    [Column] public long BattlesPlayed { get; set; }

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
