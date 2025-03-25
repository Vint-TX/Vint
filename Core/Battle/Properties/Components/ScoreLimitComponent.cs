using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Properties.Components;

[ProtocolId(-3048295118496552479)]
public class ScoreLimitComponent : IComponent {
    public int ScoreLimit { get; set; }
}
