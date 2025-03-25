using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Tank.Parameters.Components;

[ProtocolId(-177474741853856725)]
public class SpeedConfigComponent(
    float turnAcceleration,
    float sideAcceleration,
    float reverseAcceleration,
    float reverseTurnAcceleration
) : IComponent {
    public float ReverseAcceleration { get; set; } = reverseAcceleration;
    public float ReverseTurnAcceleration { get; set; } = reverseTurnAcceleration;
    public float SideAcceleration { get; set; } = sideAcceleration;
    public float TurnAcceleration { get; set; } = turnAcceleration;
}
