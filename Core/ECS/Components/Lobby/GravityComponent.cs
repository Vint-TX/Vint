using Vint.Core.Battles;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Lobby;

[ProtocolId(1435652501758)]
public class GravityComponent(
    GravityType gravityType
) : IComponent {
    public GravityType GravityType { get; private set; } = gravityType;
    public float Gravity { get; private set; } = BattleProperties.GravityToForce[gravityType];
}