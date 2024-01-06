using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Tank;

[ProtocolId(5166099393636831290)]
public class TankSemiActiveStateComponent : IComponent {
    public float ActivationTime { get; private set; } = 0.25f;
}