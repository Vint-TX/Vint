using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Components.Slot;

[ProtocolId(1485846320654)]
public class SlotUserItemInfoComponent(
    Common.Slot slot,
    ModuleBehaviourType behaviour
) : IComponent {
    public Common.Slot Slot { get; private set; } = slot;
    public ModuleBehaviourType ModuleBehaviourType { get; private set; } = behaviour;
    public int UpgradeLevel { get; private set; } = 1;
}
