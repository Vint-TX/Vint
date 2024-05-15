using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("ReputationStatistics")]
public class ReputationStatistics {
    [NotColumn] readonly Player _player = null!;

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))]
    public required Player Player {
        get => _player;
        init {
            _player = value;
            PlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PlayerId { get; private set; }
    [PrimaryKey(1)] public required DateOnly Date { get; init; }

    [Column] public required uint SeasonNumber { get; init; }
    [Column] public uint Reputation { get; set; }
}
