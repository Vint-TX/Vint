using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Components.Hammer;

[ProtocolId(2388237143993578319)]
public class MagazineStorageComponent(
    int currentCartridgeCount
) : IComponent {
    public int CurrentCartridgeCount { get; set; } = currentCartridgeCount;
}
