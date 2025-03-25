using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Weapons.Parameters.Components;

[ProtocolId(1437989437781)]
public class KickbackComponent(
    float kickbackForce
) : IComponent {
    public float KickbackForce { get; set; } = kickbackForce;
}
