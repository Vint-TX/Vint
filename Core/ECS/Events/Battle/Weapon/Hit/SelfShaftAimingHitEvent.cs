using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

[ProtocolId(8070042425022831807)]
public class SelfShaftAimingHitEvent : SelfHitEvent {
    public float HitPower { get; set; }

    protected override RemoteShaftAimingHitEvent RemoteEvent => new() {
        HitPower = HitPower,
        Targets = Targets,
        StaticHit = StaticHit,
        ShotId = ShotId,
        ClientTime = ClientTime
    };
}