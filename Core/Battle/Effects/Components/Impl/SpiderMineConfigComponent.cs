using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Effects.Components.Impl;

[ProtocolId(1487227856805)]
public class SpiderMineConfigComponent(
    float speed,
    float acceleration
) : IComponent {
    public float Speed { get; } = speed;
    public float Acceleration { get; } = acceleration;
}
