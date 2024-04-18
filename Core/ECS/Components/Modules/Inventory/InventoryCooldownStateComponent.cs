using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules.Inventory;

[ProtocolId(1486635434064)]
public class InventoryCooldownStateComponent(
    int cooldownTimeMs,
    DateTimeOffset cooldownStartTime
) : IComponent {
    public int CooldownTimeMs { get; set; } = cooldownTimeMs;
    public DateTimeOffset CooldownStartTime { get; } = cooldownStartTime;
}