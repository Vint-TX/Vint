using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Statistics")]
public class Statistics {
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

    [Column] public long BattlesParticipated { get; set; }
    [Column] public long AllBattlesParticipated { get; set; }
    [Column] public long AllCustomBattlesParticipated { get; set; }
    [Column] public long Victories { get; set; }
    [Column] public long Defeats { get; set; }
    [Column] public ulong CrystalsEarned { get; set; }
    [Column] public ulong XCrystalsEarned { get; set; }
    [Column] public uint Kills { get; set; }
    [Column] public uint Deaths { get; set; }
    [Column] public uint FlagsDelivered { get; set; }
    [Column] public uint FlagsReturned { get; set; }
    [Column] public uint GoldBoxesCaught { get; set; }
    [Column] public uint Shots { get; set; }
    [Column] public uint Hits { get; set; }

    [NotColumn] public double KD => Deaths == 0 ? Kills : Math.Round((double)Kills / Deaths, 2);
    [NotColumn] public double VD => Defeats == 0 ? Victories : Math.Round((double)Victories / Defeats, 2);

    public Dictionary<string, long> CollectClientSide() => new() {
        { "BATTLES_PARTICIPATED", BattlesParticipated },
        { "ALL_BATTLES_PARTICIPATED", AllBattlesParticipated },
        { "ALL_CUSTOM_BATTLES_PARTICIPATED", AllCustomBattlesParticipated },
        { "VICTORIES", Victories },
        { "DEFEATS", Defeats }
    };
}
