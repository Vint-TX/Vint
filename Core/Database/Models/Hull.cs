using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Hulls")]
public class Hull {
    [NotColumn] Player _player = null!;
    [Column] public long Xp { get; set; }
    [Column] public long SkinId { get; set; }
    [Column] public long Kills { get; set; }
    [Column] public long BattlesPlayed { get; set; }

    [PrimaryKey(1)] public long Id { get; init; }

    [Association(ThisKey = nameof(PlayerId), OtherKey = nameof(Player.Id))]
    public Player Player {
        get => _player;
        set {
            _player = value;
            PlayerId = value.Id;
        }
    }

    [PrimaryKey(0)] public long PlayerId { get; private set; }
}