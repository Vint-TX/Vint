using Vint.Core.ECS.Components;

namespace Vint.Core.Items.Components;

public class ItemsAutoIncreasePriceComponent : IComponent {
    public int StartCount { get; private set; }
    public int PriceIncreaseAmount { get; private set; }
    public int MaxAdditionalPrice { get; private set; }
}
