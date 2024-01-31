using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Damage;

[ProtocolId(1447764683298)]
public class SelfTankExplosionEvent(
    bool canDetachWeapon = false
) : IEvent {
    public bool CanDetachWeapon { get; private set; } = canDetachWeapon;
}