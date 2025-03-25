using Vint.Core.ECS.Components;

namespace Vint.Core.Items.Components;

public class ItemsBuyCountLimitComponent : IComponent {
    public int Limit { get; private set; }
}
