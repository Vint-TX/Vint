using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Limit;

[ProtocolId(-3048295118496552479)]
public class ScoreLimitComponent : IComponent {
    public int ScoreLimit { get; set; }
}
