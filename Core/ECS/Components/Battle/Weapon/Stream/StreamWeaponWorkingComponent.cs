using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(971549724137995758)]
public class StreamWeaponWorkingComponent : IComponent { // todo
    public int Time { get; private set; }
}