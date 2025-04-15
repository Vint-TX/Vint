using Vint.Core.Database.Models;

namespace Vint.Core.Server.API.Data.Player;

public record StatisticsData(
    long BattlesParticipated,
    long AllBattlesParticipated,
    long AllCustomBattlesParticipated,
    long Victories,
    long Defeats,
    ulong CrystalsEarned,
    ulong XCrystalsEarned,
    uint Kills,
    uint Deaths,
    uint FlagsDelivered,
    uint FlagsReturned,
    uint GoldBoxesCaught,
    uint Shots,
    uint Hits
) {
    public static StatisticsData FromStatistics(Statistics statistics) =>
        new(statistics.BattlesParticipated,
            statistics.AllBattlesParticipated,
            statistics.AllCustomBattlesParticipated,
            statistics.Victories,
            statistics.Defeats,
            statistics.CrystalsEarned,
            statistics.XCrystalsEarned,
            statistics.Kills,
            statistics.Deaths,
            statistics.FlagsDelivered,
            statistics.FlagsReturned,
            statistics.GoldBoxesCaught,
            statistics.Shots,
            statistics.Hits);
}
