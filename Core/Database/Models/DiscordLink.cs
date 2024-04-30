using LinqToDB;
using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("DiscordLinks")]
public class DiscordLink {
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
    [PrimaryKey(1)] public required ulong DiscordId { get; init; }

    [Column] public LinkStatus Status { get; set; } = LinkStatus.Pending;

    [Column(DataType = DataType.Text)] public string AccessToken { get; set; } = null!;
    [Column(DataType = DataType.Text)] public string RefreshToken { get; set; } = null!;
    [Column(DataType = DataType.Text)] public string StateHash { get; set; } = null!;
}

public enum LinkStatus : byte {
    Pending,
    Linked
}
