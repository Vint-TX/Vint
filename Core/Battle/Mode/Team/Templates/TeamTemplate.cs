using Vint.Core.Battle.Mode.Team.Components;
using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.Battle.Mode.Team.Templates;

[ProtocolId(1429761302402)]
public class TeamTemplate : EntityTemplate {
    public IEntity Create(TeamColor teamColor) => Entity(null,
        builder => builder
            .AddComponent<TeamComponent>()
            .AddComponent<TeamScoreComponent>()
            .AddComponent(new TeamColorComponent(teamColor))
            .AddGroupComponent<TeamGroupComponent>());
}
