using Vint.Core.ECS.Enums;

namespace Vint.Core.ECS.Components.Server.Modules;

public class SlotToTankPartComponent : IComponent {
    public Dictionary<Slot, TankPartModuleType> Slots { get; private set; } = null!;
}
