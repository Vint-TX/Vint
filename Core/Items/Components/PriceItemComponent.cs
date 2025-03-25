using Vint.Core.ECS.Components;

namespace Vint.Core.Items.Components;

public class PriceItemComponent : IComponent {
    public int Price { get; private set; }
}
