using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("ReputationStatistics")]
public class ReputationStatistics {
    [NotColumn] Player _player = null!;

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))]
    public Player Player {
        get => _player;
        set {
            _player = value;
            PlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PlayerId { get; private set; }
    [PrimaryKey(1)] public DateOnly Date { get; init; }

    [Column] public uint SeasonNumber { get; init; }
    [Column] public ulong Reputation { get; set; }
}