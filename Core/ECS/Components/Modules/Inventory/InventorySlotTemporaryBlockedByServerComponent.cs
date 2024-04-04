using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules.Inventory;

[ProtocolId(636367520290400984)]
public class InventorySlotTemporaryBlockedByServerComponent(
    long blockTimeMs,
    DateTimeOffset startBlockTime
) : IComponent {
    public long BlockTimeMs { get; } = blockTimeMs;
    public DateTimeOffset StartBlockTime { get; } = startBlockTime;
}