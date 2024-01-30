using Vint.Core.ECS.Enums;
using Vint.Core.Protocol.Attributes;

namespace Vint.Core.ECS.Components.Battle.Team;

[ProtocolId(6258344835131144773)]
public class TeamColorComponent(
    TeamColor color
) : IComponent {
    public TeamColor TeamColor { get; set; } = color;
}