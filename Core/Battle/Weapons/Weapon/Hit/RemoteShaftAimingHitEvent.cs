using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Weapon.Hit;

[ProtocolId(4743444303755604700)]
public class RemoteShaftAimingHitEvent : RemoteHitEvent {
    public float HitPower { get; set; }
}
