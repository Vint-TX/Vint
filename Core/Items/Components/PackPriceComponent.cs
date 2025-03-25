using Vint.Core.ECS.Components;

namespace Vint.Core.Items.Components;

public class PackPriceComponent : IComponent {
    public Dictionary<int, int> PackPrice { get; private set; } = null!;
    public Dictionary<int, int> PackXPrice { get; private set; } = null!;
}
