using Vint.Core.ECS.Components;

namespace Vint.Core.Items.Components;

public class FirstBuySaleComponent : IComponent {
    public int SalePercent { get; private set; }
}
