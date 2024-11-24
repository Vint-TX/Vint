using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Modules")]
public class Module {
    [PrimaryKey(1)] public required long Id { get; init; }

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))] [field: NotColumn]
    public required Player Player {
        get;
        init {
            field = value;
            PlayerId = value.Id;
        }
    } = null!;

    [PrimaryKey(0)] public long PlayerId { get; private set; }

    [Column] public int Level { get; set; } = -1;
    [Column] public int Cards { get; set; }
}
