using Vint.Core.ECS.Components;

namespace Vint.Core.Items.Components;

public class RanksExperiencesConfigComponent : IComponent {
    public List<int> RanksExperiences { get; private set; } = null!;
}
