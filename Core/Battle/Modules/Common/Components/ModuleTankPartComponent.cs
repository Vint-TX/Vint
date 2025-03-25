using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Modules.Common.Components;

[ProtocolId(636324457894395944)]
public class ModuleTankPartComponent(
    TankPartModuleType part
) : IComponent {
    public TankPartModuleType Part { get; } = part;
}
