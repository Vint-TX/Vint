using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(1498352458940656986), ClientAddable, ClientRemovable]
public class StreamWeaponIdleComponent : IComponent {
    public int Time { get; private set; }
}