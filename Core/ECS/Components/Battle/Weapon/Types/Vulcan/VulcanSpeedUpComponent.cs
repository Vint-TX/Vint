using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Vulcan;

[ProtocolId(-7317457627241247550), ClientAddable, ClientRemovable]
public class VulcanSpeedUpComponent : IComponent {
    public int ClientTime { get; private set; }
}