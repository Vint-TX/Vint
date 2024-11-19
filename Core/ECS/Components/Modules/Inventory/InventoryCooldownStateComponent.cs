using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules.Inventory;

[ProtocolId(1486635434064)]
public class InventoryCooldownStateComponent(
    TimeSpan cooldownTime,
    DateTimeOffset cooldownStartTime
) : IComponent {
    [ProtocolTimeKind<int>(TimeSpanKind.Milliseconds)]
    public TimeSpan CooldownTimeMs { get; set; } = cooldownTime;
    public DateTimeOffset CooldownStartTime { get; } = cooldownStartTime;
}
