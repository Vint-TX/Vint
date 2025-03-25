using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Modules.Common.Components.Slot;

public class SlotToTankPartComponent : IComponent {
    public Dictionary<Common.Slot, TankPartModuleType> Slots { get; private set; } = null!;
}
