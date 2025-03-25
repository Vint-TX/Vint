namespace Vint.Core.ECS.Components.Experience;

public class UpgradeLevelsComponent : IComponent {
    public int[] LevelsExperiences { get; private set; } = null!;
}
