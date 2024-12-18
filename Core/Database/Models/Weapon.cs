using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table(DbConstants.Weapons)]
public class Weapon {
    [PrimaryKey(1)] public required long Id { get; init; }

    [Column] public required long SkinId { get; init; } // ??
    [Column] public required long ShellId { get; init; } // ??
    [Column] public long Xp { get; set; }
    [Column] public long Kills { get; set; }
    [Column] public long BattlesPlayed { get; set; }

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
