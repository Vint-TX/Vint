using LinqToDB;
using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("PlayersPreferences")]
public class PlayerPreferences {
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

    [Column] public bool RememberMe { get; set; }

    [Column] public bool Subscribed { get; set; }
    [Column] public bool DiscordConfirmed { get; set; }
    [Column(DataType = DataType.Text)] public string CountryCode { get; set; } = "RU";
}
