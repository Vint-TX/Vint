using Vint.Core.ECS.Enums;

namespace Vint.Core.ECS.Components.Server;

public class SlotsTypesComponent : IComponent {
    public Dictionary<Slot, ModuleBehaviourType> Slots { get; private set; } = null!;
}
