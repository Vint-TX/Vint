using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

[ProtocolId(4743444303755604700)]
public class RemoteShaftAimingHitEvent : RemoteHitEvent {
    public float HitPower { get; set; }
}