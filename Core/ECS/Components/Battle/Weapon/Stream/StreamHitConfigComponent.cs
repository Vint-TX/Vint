using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Weapon.Stream;

[ProtocolId(-5407563795844501148)]
public class StreamHitConfigComponent(
    float localCheckPeriod,
    float sendToServerPeriod,
    bool detectStaticHit
) : IComponent {
    public float LocalCheckPeriod { get; set; } = localCheckPeriod;
    public float SendToServerPeriod { get; set; } = sendToServerPeriod;
    public bool DetectStaticHit { get; set; } = detectStaticHit;
}