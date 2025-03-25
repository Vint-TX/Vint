using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Vulcan;

[ProtocolId(-7317457627241247550), ClientAddable, ClientRemovable]
public class VulcanSpeedUpComponent : IComponent {
    public int ClientTime { get; private set; }
}
