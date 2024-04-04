using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules.Slot;

[ProtocolId(636326081851010949)]
public class SlotTankPartComponent(
    TankPartModuleType part
) : IComponent {
    public TankPartModuleType TankPart { get; private set; } = part;
}