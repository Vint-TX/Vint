using Vint.Core.ECS.Components;

namespace Vint.Core.Items.Components;

public class UpgradeLevelsComponent : IComponent {
    public int[] LevelsExperiences { get; private set; } = null!;
}
