using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Weapon.Hit;

[ProtocolId(-2203330189936241204)]
public class RemoteSplashHitEvent : RemoteHitEvent {
    public List<HitTarget>? SplashTargets { get; set; }
}