using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Components.Inventory;

[ProtocolId(636366605665347423)]
public class BattleUserInventoryCooldownSpeedComponent(
    float speedCoeff
) : IComponent {
    public float SpeedCoeff { get; set; } = speedCoeff;
}
