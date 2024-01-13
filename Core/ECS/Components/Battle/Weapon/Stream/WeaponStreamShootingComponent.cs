using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(6803807621463709653)]
public class WeaponStreamShootingComponent : IComponent { // todo
    public DateTimeOffset? StartShootingTime { get; private set; }
    public int Time { get; private set; }
}