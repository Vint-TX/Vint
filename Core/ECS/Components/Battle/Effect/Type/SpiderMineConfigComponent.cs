using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Effect.Type;

[ProtocolId(1487227856805)]
public class SpiderMineConfigComponent(
    float speed,
    float acceleration
) : IComponent {
    public float Speed { get; } = speed;
    public float Acceleration { get; } = acceleration;
}
