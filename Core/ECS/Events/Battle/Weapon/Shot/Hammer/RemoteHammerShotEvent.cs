using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle.Weapon.Shot.Hammer;

[ProtocolId(-8245726943400840523)]
public class RemoteHammerShotEvent : RemoteShotEvent {
    public int RandomSeed { get; set; }
}
