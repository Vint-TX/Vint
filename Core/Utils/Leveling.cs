using Vint.Core.Config;
using Vint.Core.Database;
using Vint.Core.ECS.Components.Experience;
using Vint.Core.ECS.Components.Server.Experience;

namespace Vint.Core.Utils;

public static class Leveling {
    public static int GetRank(long xp) {
        List<int> xpPerRank = new(101) { 0 };

        xpPerRank.AddRange(ConfigManager.GetComponent<RanksExperiencesConfigComponent>("ranksconfig").RanksExperiences);
        xpPerRank = xpPerRank.OrderBy(x => x).ToList();

        return xpPerRank.IndexOf(xpPerRank.LastOrDefault(x => x <= xp)) + 1;
    }

    public static int GetLevel(long xp) {
        List<int> experiencePerLevel = [0];
        experiencePerLevel.AddRange(ConfigManager.GetComponent<UpgradeLevelsComponent>("garage").LevelsExperiences);

        int levelIndex = experiencePerLevel.IndexOf(experiencePerLevel.LastOrDefault(x => x <= xp));

        return levelIndex + 1;
    }

    public static int GetSeasonPlace(long userId) {
        using DbConnection db = new();

        return db.SeasonStatistics
                   .Select(seasonStats => new { Id = seasonStats.PlayerId, seasonStats.Reputation })
                   .OrderByDescending(p => p.Reputation)
                   .Select((player, index) => new { player.Id, Index = index })
                   .Single(p => p.Id == userId)
                   .Index +
               1;
    }
}