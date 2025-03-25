using Vint.Core.Config;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Items.Components;

[ProtocolId(1438924983080)]
public class ExperienceToLevelUpItemComponent : IComponent {
    public ExperienceToLevelUpItemComponent(long xp) {
        List<int> experiencePerRank = [0];

        experiencePerRank.AddRange(ConfigManager.GetComponent<UpgradeLevelsComponent>("garage")
            .LevelsExperiences);

        int rankIndex = experiencePerRank.IndexOf(experiencePerRank.LastOrDefault(x => x <= xp));

        InitLevelExperience = experiencePerRank[rankIndex];

        FinalLevelExperience = experiencePerRank[rankIndex +
                                                 (rankIndex >= 8
                                                     ? 0
                                                     : 1)];

        RemainingExperience = Convert.ToInt32(FinalLevelExperience - xp);
    }

    public int InitLevelExperience { get; private set; }
    public int FinalLevelExperience { get; private set; }
    public int RemainingExperience { get; private set; }
}
