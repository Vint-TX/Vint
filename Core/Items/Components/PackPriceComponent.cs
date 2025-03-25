namespace Vint.Core.ECS.Components.Server.Shop;

public class PackPriceComponent : IComponent {
    public Dictionary<int, int> PackPrice { get; private set; } = null!;
    public Dictionary<int, int> PackXPrice { get; private set; } = null!;
}
