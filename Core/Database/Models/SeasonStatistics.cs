using LinqToDB.Mapping;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Database.Models;

[Table("SeasonStatistics")]
public class SeasonStatistics {
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
    [PrimaryKey(1)] public required uint SeasonNumber { get; init; }

    [Column] public required uint Reputation { get; set; }
    [Column] public uint BattlesPlayed { get; set; }
    [Column] public uint Kills { get; set; }
    [Column] public uint ExperienceEarned { get; set; }
    [Column] public uint CrystalsEarned { get; set; }
    [Column] public uint XCrystalsEarned { get; set; }
    [Column] public uint ContainersOpened { get; set; }

    [NotColumn] public int LeagueIndex => Reputation switch {
        < 139 => 0,
        < 999 => 1,
        < 2999 => 2,
        < 4499 => 3,
        < 99999 => 4,
        _ => 4
    };

    [NotColumn] public string LeagueName => LeagueIndex switch {
        0 => "Training",
        1 => "Bronze",
        2 => "Silver",
        3 => "Gold",
        4 => "Master",
        _ => "Master"
    };

    [NotColumn] public IEntity League => GlobalEntities.GetEntity("leagues", LeagueName);
}
