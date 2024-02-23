using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Item;

[ProtocolId(1479806073802)]
public class ItemsContainerItemComponent : IComponent {
    public List<ContainerItem> Items { get; set; } = null!;
    public List<ContainerItem>? RareItems { get; set; }
}

public class ContainerItem : IComponent {
    public List<MarketItemBundle> ItemBundles { get; set; } = null!;
    public long Compensation { get; set; }
    public string NameLocalizationKey { get; set; } = "";
}

public class MarketItemBundle : IComponent {
    public long MarketItem { get; set; }
    public int Amount { get; set; } = 1;
    public int Max { get; set; } = 1;
}