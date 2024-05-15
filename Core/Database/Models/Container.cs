using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Containers")]
public class Container {
    [NotColumn] readonly Player _player = null!;
    [PrimaryKey(1)] public required long Id { get; init; }

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))]
    public Player Player {
        get => _player;
        init {
            _player = value;
            PlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PlayerId { get; private set; }

    [Column] public required long Count { get; set; }
}
