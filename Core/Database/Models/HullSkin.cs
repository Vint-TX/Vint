using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table(DbConstants.HullSkins)]
public class HullSkin {
    [PrimaryKey(2)] public required long Id { get; init; }

    [PrimaryKey(1)] public required long HullId { get; init; }

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
