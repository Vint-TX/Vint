using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules.Slot;

[ProtocolId(1485846320654)]
public class SlotUserItemInfoComponent(
    Enums.Slot slot,
    ModuleBehaviourType behaviour
) : IComponent {
    public Enums.Slot Slot { get; private set; } = slot;
    public ModuleBehaviourType ModuleBehaviourType { get; private set; } = behaviour;
    public int UpgradeLevel { get; private set; } = 1;
}