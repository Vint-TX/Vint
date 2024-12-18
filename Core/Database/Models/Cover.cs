using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table(DbConstants.Covers)]
public class Cover {
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
}
