using Vint.Core.ECS.Components;

namespace Vint.Core.Battle.Modules.Common.Components.Slot;

public class SlotsTypesComponent : IComponent {
    public Dictionary<Common.Slot, ModuleBehaviourType> Slots { get; private set; } = null!;
}
