using Vint.Core.ECS.Components;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Mode.Team.Components;

[ProtocolId(6258344835131144773)]
public class TeamColorComponent(
    TeamColor color
) : IComponent {
    public TeamColor TeamColor { get; set; } = color;
}
