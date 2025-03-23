using Vint.Core.ECS.Components.Group;
using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Templates.Squad;

[ProtocolId(1507120664314)]
public class SquadTemplate : EntityTemplate {
    public IEntity Create() =>
        Entity("/squad", builder => builder.AddGroupComponent<SquadGroupComponent>());
}
