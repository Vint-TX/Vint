using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Containers")]
public class Container {
    [PrimaryKey(1)] public required long Id { get; init; }

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))] [field: NotColumn]
    public Player Player {
        get;
        init {
            field = value;
            PlayerId = value.Id;
        }
    } = null!;

    [PrimaryKey(0)] public long PlayerId { get; private set; }

    [Column] public required long Count { get; set; }
}
