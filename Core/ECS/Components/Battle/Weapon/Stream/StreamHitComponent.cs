using Vint.Core.ECS.Events.Battle.Weapon.Hit;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(-6274985110858845212), ClientAddable, ClientRemovable]
public class StreamHitComponent : IComponent {
    public HitTarget? TankHit { get; private set; }
    public StaticHit? StaticHit { get; private set; }
}