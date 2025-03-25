using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Components.Slot;

[ProtocolId(636326081851010949)]
public class SlotTankPartComponent(
    TankPartModuleType part
) : IComponent {
    public TankPartModuleType TankPart { get; private set; } = part;
}
