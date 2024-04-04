using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules.Inventory;

[ProtocolId(636366605665347423)]
public class BattleUserInventoryCooldownSpeedComponent(
    float speedCoeff
) : IComponent {
    public float SpeedCoeff { get; set; } = speedCoeff;
}