using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Mode.Team.Components;

[ProtocolId(-2440064891528955383)]
public class TeamScoreComponent : IComponent {
    public int Score { get; set; }
}
