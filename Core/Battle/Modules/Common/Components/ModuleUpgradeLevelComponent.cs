using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Components;

[ProtocolId(636329487716905336)]
public class ModuleUpgradeLevelComponent(
    long level
) : IComponent {
    public long Level { get; set; } = level;
}
