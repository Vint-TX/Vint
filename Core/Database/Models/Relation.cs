using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Relations")]
public class Relation {
    [NotColumn] Player _sourcePlayer = null!;
    [NotColumn] Player _targetPlayer = null!;

    [Association(ThisKey = nameof(SourcePlayerId), OtherKey = nameof(Player.Id))]
    public Player SourcePlayer {
        get => _sourcePlayer;
        set {
            _sourcePlayer = value;
            SourcePlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long SourcePlayerId { get; private set; }

    [Association(ThisKey = nameof(TargetPlayerId), OtherKey = nameof(Player.Id))]
    public Player TargetPlayer {
        get => _targetPlayer;
        set {
            _targetPlayer = value;
            TargetPlayerId = value.Id;
        }
    }

    [PrimaryKey(1)] public long TargetPlayerId { get; private set; }

    [Column] public RelationTypes Types { get; set; }
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