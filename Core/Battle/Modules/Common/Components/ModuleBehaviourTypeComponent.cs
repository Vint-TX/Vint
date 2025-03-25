using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Components;

[ProtocolId(636341573884178402)]
public class ModuleBehaviourTypeComponent(
    ModuleBehaviourType behaviour
) : IComponent {
    public ModuleBehaviourType BehaviourType { get; private set; } = behaviour;
}
