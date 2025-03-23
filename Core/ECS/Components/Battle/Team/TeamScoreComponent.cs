using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Team;

[ProtocolId(-2440064891528955383)]
public class TeamScoreComponent : IComponent {
    public int Score { get; set; }
}
