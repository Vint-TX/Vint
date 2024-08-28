using Vint.Core.ECS.Enums;

namespace Vint.Core.ECS.Components.Server;

public class SlotToTankPartComponent : IComponent {
    public Dictionary<Slot, TankPartModuleType> Slots { get; private set; } = null!;
}
