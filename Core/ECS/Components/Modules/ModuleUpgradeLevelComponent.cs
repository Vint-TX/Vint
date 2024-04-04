using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules;

[ProtocolId(636329487716905336)]
public class ModuleUpgradeLevelComponent(
    long level
) : IComponent {
    public long Level { get; set; } = level;
}