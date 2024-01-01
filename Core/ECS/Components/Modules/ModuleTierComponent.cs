using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules;

[ProtocolId(636330378478033958)]
public class ModuleTierComponent(
    int tier
) : IComponent {
    public int TierNumber { get; private set; } = tier;
}