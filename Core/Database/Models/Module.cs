using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Modules")]
public class Module {
    [NotColumn] readonly Player _player = null!;
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

    [Column] public int Level { get; set; } = -1;
    [Column] public int Cards { get; set; }
}
