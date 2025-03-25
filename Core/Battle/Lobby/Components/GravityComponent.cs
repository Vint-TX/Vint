using Vint.Core.Battle.Properties;
using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Lobby.Components;

[ProtocolId(1435652501758)]
public class GravityComponent(
    GravityType gravityType
) : IComponent {
    public GravityType GravityType { get; private set; } = gravityType;
    public float Gravity { get; private set; } = GravityToForce[gravityType];

    [ProtocolIgnore]
    static Dictionary<GravityType, float> GravityToForce { get; } = new(4) {
        { GravityType.Earth, 9.81f },
        { GravityType.Moon, 1.62f },
        { GravityType.Mars, 3.71f },
        { GravityType.SuperEarth, 30f }
    };
}
