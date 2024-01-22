using System.Text;
using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Punishments")]
public class Punishment {
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
    [PrimaryKey(1), Identity] public long Id { get; set; }
    
    [Column] public PunishmentType Type { get; init; }
    [Column] public DateTimeOffset PunishTime { get; init; }
    [Column] public TimeSpan? Duration { get; init; }
    
    [Column] public string? Reason { get; init; }
    [Column] public bool Active { get; set; }
    
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