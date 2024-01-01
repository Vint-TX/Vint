using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules;

[ProtocolId(636341573884178402)]
public class ModuleBehaviourTypeComponent(
    ModuleBehaviourType behaviour
) : IComponent {
    public ModuleBehaviourType BehaviourType { get; private set; } = behaviour;
}