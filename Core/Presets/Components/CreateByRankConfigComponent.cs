using Vint.Core.ECS.Components;

namespace Vint.Core.Presets.Components;

public class CreateByRankConfigComponent : IComponent {
    public List<int> UserRankListToCreateItem { get; private set; } = null!;
}
