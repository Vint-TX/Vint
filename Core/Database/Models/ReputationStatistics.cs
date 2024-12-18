using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table(DbConstants.ReputationStatistics)]
public class ReputationStatistics {
    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))] [field: NotColumn]
    public required Player Player {
        get;
        init {
            field = value;
            PlayerId = value.Id;
        }
    } = null!;

    [PrimaryKey(0)] public long PlayerId { get; private set; }
    [PrimaryKey(1)] public required DateOnly Date { get; init; }

    [Column] public required uint SeasonNumber { get; init; }
    [Column] public uint Reputation { get; set; }
}
