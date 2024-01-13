using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Types.Hammer;

[ProtocolId(2388237143993578319)]
public class MagazineStorageComponent(
    int currentCartridgeCount
) : IComponent {
    public int CurrentCartridgeCount { get; set; } = currentCartridgeCount;
}