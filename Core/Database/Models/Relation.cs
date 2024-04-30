using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Relations")]
public class Relation {
    [NotColumn] readonly Player _sourcePlayer = null!;
    [NotColumn] readonly Player _targetPlayer = null!;

    [Association(ThisKey = nameof(SourcePlayerId), OtherKey = nameof(Player.Id))]
    public required Player SourcePlayer {
        get => _sourcePlayer;
        init {
            _sourcePlayer = value;
            SourcePlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long SourcePlayerId { get; private set; }

    [Association(ThisKey = nameof(TargetPlayerId), OtherKey = nameof(Player.Id))]
    public required Player TargetPlayer {
        get => _targetPlayer;
        init {
            _targetPlayer = value;
            TargetPlayerId = value.Id;
        }
    }

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
