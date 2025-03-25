using Vint.Core.ECS.Entities;
using Vint.Core.ECS.Templates;
using Vint.Core.Server.Game.Protocol.Attributes;
using Vint.Core.Squads.Components;

namespace Vint.Core.Squads.Templates;

[ProtocolId(1507120664314)]
public class SquadTemplate : EntityTemplate {
    public IEntity Create() =>
        Entity("/squad", builder => builder.AddGroupComponent<SquadGroupComponent>());
}
