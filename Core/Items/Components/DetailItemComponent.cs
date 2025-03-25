namespace Vint.Core.ECS.Components.Server.Shop;

public class DetailItemComponent : IComponent {
    public long TargetMarketItemId { get; private set; }
    public int RequiredCount { get; private set; }
}
