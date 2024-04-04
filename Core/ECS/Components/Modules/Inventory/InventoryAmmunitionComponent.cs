using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules.Inventory;

[ProtocolId(636383014039871905)]
public class InventoryAmmunitionComponent(
    int maxCount,
    int currentCount
) : IComponent {
    public InventoryAmmunitionComponent(int maxCount) : this(maxCount, maxCount) { }

    public int MaxCount { get; } = maxCount;
    public int CurrentCount { get; set; } = currentCount;
}