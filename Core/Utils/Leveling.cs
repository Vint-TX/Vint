using Vint.Core.Config;
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
        List<int> experiencePerRank = [0];
        experiencePerRank.AddRange(ConfigManager.GetComponent<UpgradeLevelsComponent>("garage").LevelsExperiences);
        
        int rankIndex = experiencePerRank.IndexOf(experiencePerRank.LastOrDefault(x => x <= xp));

        return rankIndex + 1;
    }
}