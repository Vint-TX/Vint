using System.Text;
using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Punishments")]
public class Punishment {
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
    [PrimaryKey(1), Identity] public long Id { get; set; }

    [Column] public string? IPAddress { get; init; }
    [Column] public required string HardwareFingerprint { get; init; }
    [Column] public required PunishmentType Type { get; init; }
    [Column] public required DateTimeOffset PunishTime { get; init; }
    [Column] public required TimeSpan? Duration { get; init; }

    [Column] public required string? Reason { get; init; }
    [Column] public required bool Active { get; set; }

    [NotColumn] public bool Permanent => Duration == null;
    [NotColumn] public DateTimeOffset? EndTime => PunishTime + Duration;

    public override string ToString() {
        string verb = Type switch {
            PunishmentType.Warn => "warned",
            PunishmentType.Mute => "muted",
            PunishmentType.Ban => "banned",
            _ => ""
        };

        StringBuilder builder = new(verb);

        if (Reason != null) {
            builder.Append(" for \"");
            builder.Append(Reason);
            builder.Append('"');
        }

        if (EndTime != null) {
            builder.Append(" until ");
            builder.Append(EndTime);
        }

        return builder.ToString();
    }
}

public enum PunishmentType {
    Warn,
    Mute,
    Ban
}
