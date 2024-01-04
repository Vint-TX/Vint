using LinqToDB.Mapping;
using Vint.Core.ECS.Entities;

namespace Vint.Core.Database.Models;

[Table("SeasonStatistics")]
public class SeasonStatistics {
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
    [PrimaryKey(1)] public int SeasonNumber { get; init; }

    [Column] public int Reputation { get; set; }
    [Column] public int BattlesPlayed { get; set; }
    [Column] public int Kills { get; set; }
    [Column] public int ExperienceEarned { get; set; }
    [Column] public int CrystalsEarned { get; set; }
    [Column] public int XCrystalsEarned { get; set; }
    [Column] public int ContainersOpened { get; set; }

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