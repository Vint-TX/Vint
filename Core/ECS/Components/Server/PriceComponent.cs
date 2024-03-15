namespace Vint.Core.ECS.Components.Server;

public class PriceItemComponent : IComponent {
    public int Price { get; private set; }
}

public class XPriceItemComponent : IComponent {
    public int Price { get; private set; }
}

public class FirstBuySaleComponent : IComponent {
    public int SalePercent { get; private set; }
}

public class CreateByRankConfigComponent : IComponent {
    public List<int> UserRankListToCreateItem { get; private set; } = null!;
}

public class ItemsBuyCountLimitComponent : IComponent {
    public int Limit { get; private set; }
}

public class ItemsAutoIncreasePriceComponent : IComponent {
    public int StartCount { get; private set; }
    public int PriceIncreaseAmount { get; private set; }
    public int MaxAdditionalPrice { get; private set; }
}