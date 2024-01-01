using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Modules;

[ProtocolId(636324457894395944)]
public class ModuleTankPartComponent(
    TankPartModuleType part
) : IComponent {
    public TankPartModuleType Part { get; private set; } = part;
}