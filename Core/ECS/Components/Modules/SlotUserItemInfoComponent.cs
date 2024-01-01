using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules;

[ProtocolId(1485846320654)]
public class SlotUserItemInfoComponent(
    Slot slot,
    ModuleBehaviourType behaviour
) : IComponent {
    public Slot Slot { get; private set; } = slot;
    public ModuleBehaviourType ModuleBehaviourType { get; private set; } = behaviour;
    public int UpgradeLevel { get; private set; } = 1;
}