using Vint.Core.ECS.Components;

namespace Vint.Core.Items.Components;

public class DetailItemComponent : IComponent {
    public long TargetMarketItemId { get; private set; }
    public int RequiredCount { get; private set; }
}
