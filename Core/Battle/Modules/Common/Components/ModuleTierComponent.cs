using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Components;

[ProtocolId(636330378478033958)]
public class ModuleTierComponent(
    int tier
) : IComponent {
    public int TierNumber { get; private set; } = tier;
}
