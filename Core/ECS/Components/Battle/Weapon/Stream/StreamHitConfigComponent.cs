using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(-5407563795844501148)]
public class StreamHitConfigComponent : IComponent {
    public float LocalCheckPeriod { get; private set; }
    public float SendToServerPeriod { get; private set; }
    public bool DetectStaticHit { get; private set; }
}
