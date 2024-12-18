using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table(DbConstants.Relations)]
public class Relation {
    [Association(ThisKey = nameof(SourcePlayerId), OtherKey = nameof(Player.Id))] [field: NotColumn]
    public required Player SourcePlayer {
        get;
        init {
            field = value;
            SourcePlayerId = value.Id;
        }
    } = null!;

    [PrimaryKey(0)] public long SourcePlayerId { get; private set; }

    [Association(ThisKey = nameof(TargetPlayerId), OtherKey = nameof(Player.Id))] [field: NotColumn]
    public required Player TargetPlayer {
        get;
        init {
            field = value;
            TargetPlayerId = value.Id;
        }
    } = null!;

    [PrimaryKey(1)] public long TargetPlayerId { get; private set; }

    [Column] public required RelationTypes Types { get; set; }
}

[Flags]
public enum RelationTypes {
    None = 0,

    Blocked = 1,
    Reported = 2,

    Friend = 4,
    IncomingRequest = 8,
    OutgoingRequest = 16
}
