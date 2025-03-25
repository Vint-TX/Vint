namespace Vint.Core.ECS.Components.Server.Shop;

public class ItemsAutoIncreasePriceComponent : IComponent {
    public int StartCount { get; private set; }
    public int PriceIncreaseAmount { get; private set; }
    public int MaxAdditionalPrice { get; private set; }
}
